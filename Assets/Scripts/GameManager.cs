﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;
using Volpi.Entertaiment.SDK.Utilities;
using Volpi.Entertainment.SDK.Utilities;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("------------------------------------Player Settings------------------------------------")]
    public AnimationCurve PlayerJumpCurve;
    public AnimationCurve PlayerBounceScaleXCurve, PlayerBounceScaleYCurve;

    public float PlayerAnimationSpeed;
    public float playerPositionOnZAxis;
    public Vector3 playerScaleWhenItIsBreakingPlatforms;
    
    [Header("-------------------------------Step Settings-------------------------------")]
    public int stepGenerationCountOnInitializing;
    public float stepBreakingSoundEffectPitchMultiplier;
    public float stepDestroyTime;
    public float stepYAxisForceMultiplier;
    public float stepXAxisForceMultiplier;
    public float stepXAxisForceBreakingTimeMultiplier;
    public float stepTorqueMultiplier;
    [Header("------------------------------------Meteor Settings------------------------------------")]
    public float MeteorGenerateTimeCounterRandomMax;
    public float MeteorDestroyTime;
    public float MeteorXAxisVelocityMin, MeteorXAxisVelocityMax, MeteorYAxisVelocityMin, MeteorYAxisVelocityMax;
    [Header("-------------------------------Instantiate Object Samples-------------------------------")]
    public GameObject PlayerSample;
    public GameObject CrackedPlayerSample;
    public GameObject MeteorSample, CenterCylinderSample;
    public GameObject endingPlatformSample;
    [Header("------------------------------------Sound Effects------------------------------------")]
    public AudioClip breakSoundClip;
    public AudioClip bounceSoundClip, successSoundClip, meteorBreakSoundClip, playerBreakSoundClip;
    [Header("------------------------------------Camera Settings------------------------------------")]
    public Transform CameraOrigin;
    public Vector3 CameraGameStartingPosition;
    public Transform Camera;
    public Light sceneLight;
    public float CameraShakeMultiplier;
    public float CameraRotateDegreeWhenPlayerIsMeteor;
    [Header("------------------------------------Background Settings------------------------------------")]
    public AnimationCurve sceneLightCurve;
    public Gradient backgroundBottomColorTransient, backgroundTopColorTransient;
    public BackgroundColorPair[] BackgroundRandomColorPairs;
    public Color overrideBottom, overrideTop;
    [Header("-------------------------------Breaking Bonus Settings-------------------------------")]
    public Image breakBonus;
    public float breakingBonusMax;
    public GameObject breakBonusWarning;
    public Gradient breakBonusColor;
    public float breakingBonusIncreasingSpeed, breakingBonusDecreasingSpeed, breakingBonusDecreasingSpeedWhenMeteor;
    [Header("-------------------------------Splash Decay Settings-------------------------------")]
    public Sprite[] splashSpriteSamples;
    public float splashSpriteScale;
    [Header("----------------------------------Score Settings----------------------------------")]
    public Text scoreValueText;
    public Gradient scoreTextColorGradient;
    public float scoreIncreaseAmount, scoreIncreaseAmountWhenPlayerIsMeteor, scoreScaleWhenMeteorModeActive;
    [Header("-------------------------------Level Indicator Settings-------------------------------")]
    public Slider gameStateSlider;
    public Text levelIndicatorSource, levelIndicatorTarget;
    [Header("-------------------------------Game End UI Settings-------------------------------")]
    public GameEndUI gameEndUI;
    [Header("-------------------------------Second Chance Settings-------------------------------")]
    public bool isSecondChanceActive;
    public int secondChanceTimeCounterMax, secondChanceLimit;
    [Header("-------------------------------Live Auto Generator Settings-------------------------------")]
    public bool isLiveAutoGeneratorActive;
    public int autoGeneratorSeed;
    public AnimationCurve autoGeneratorHardnessCurve;
    public List<AutoGeneratorColorPair> AutoGeneratorColorPairs;
    public List<AnimationCurve> AutoGenerateScales;
    public float speedToAngle, speedTrapOccurenceDecreaser, trapOccurenceDecreaser;
    public int maxHardnessLevelNumber;
    public AnimationCurve totalStepMinCurve, totalStepMaxCurve;
    [Header("-------------------------------Other Settings-------------------------------")]
    public GameObject GameStartUI;
    public bool isToleranceEnabled;
    public float hittingToleranceAmount;
    //Start of Variables which are not shown in Inspector
    public Step[] platformSamples;
    List<StepReferenceToGame> generatedLevelSteps = new List<StepReferenceToGame>();
    Queue<StepGeneration> stepGenerationQueue = new Queue<StepGeneration>();
    Transform Player, CrackedPlayer;
    Transform GeneratedObjects, GarbageObjects;
    float playerTimeCounter, bonusBreakingTime = 0;
    bool isBreakingBonusConsuming;
    Transform StarParent;
    Transform CenterCylinder;
    float meteorTimeCounter, meteorRandomValue, starVisibilityTime;
    float levelEnding;
    bool isNotFirstMovingSteps;
    bool isEndingPlatformGenerated;
    bool isGameEnded;
    Vector3 cameraShakeAmount;
    Coroutine secondChanceCoroutine;
    enum EndingTypes { NONE, COMPLETE, PASSEDALLLEVELS, GAMEOVER };
    EndingTypes endingType = EndingTypes.NONE;
    int currentLevel;
    float scoreValue;
    int secondChanceLimitCounter;
    bool creditOpenedBlocker;
    bool isGameStartMenuDisabled;
    bool isPlayerTouching;
    Transform toleratedStep;
    bool isToleranceEffectActivated;
    bool isVibrationEnabled;
    bool isMusicEnabled;
    
    
    private void Start()
    {
        LoadLevel(1); 
        meteorRandomValue = Random.Range(0, MeteorGenerateTimeCounterRandomMax);
    }
    
    private void Update()
    {
        StepUpdate(); 
        
        if (Player != null)
        {
            PlayerUpdate();
        }
    }
    
    public void LoadLevel(int levelNumber)
    {
        scoreValue = 0;
        secondChanceLimitCounter = secondChanceLimit;
        scoreValueText.text = "0";
        bonusBreakingTime = 0;
        isBreakingBonusConsuming = false;
        gameStateSlider.value = gameStateSlider.minValue;
        currentLevel = levelNumber;
        endingType = EndingTypes.NONE;
        isGameEnded = false;
        isEndingPlatformGenerated = false;
        levelEnding = 0;
        Camera.localPosition = CameraGameStartingPosition;
        isToleranceEffectActivated = false;
        toleratedStep = null;
        generatedLevelSteps.Clear();
        stepGenerationQueue.Clear();
        
        if (isVibrationEnabled)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // TODO Vibration
            Vibration.Cancel();
#endif
        }
        // End of Section 2

        if (Player != null) Destroy(Player.gameObject); // Section 3 - Comment: Check object exists, if there is, destroy it or don't do anything and generate the new ones.
        Player = Instantiate<GameObject>(PlayerSample).transform;

        if (CrackedPlayer != null) Destroy(CrackedPlayer.gameObject);
        CrackedPlayer = null;

        if (CenterCylinder != null) Destroy(CenterCylinder.gameObject);
        CenterCylinder = Instantiate<GameObject>(CenterCylinderSample).transform;

        if (GarbageObjects != null) Destroy(GarbageObjects.gameObject);
        GarbageObjects = new GameObject("GarbageObjects").transform; // End of Section 3

        if (levelNumber != -1) // Section 4 - Comment: If levelNumber isn't -1, that means, the GameScene is opened, and do Section 3.
        {
            if (GeneratedObjects != null) Destroy(GeneratedObjects.gameObject);
            GeneratedObjects = new GameObject("GeneratedObjects").transform;
        } // End of Section 4
        

        if (levelNumber == -1) // Means the LevelCreatorScene is opened
        {
            GeneratedObjects = GameObject.Find("GeneratedObjects").transform; // GeneratedObjects and assign it.
            for (int i = 0; i < GeneratedObjects.childCount; i++) // Section 6 - Comment: Read stepIndexes of each steps under generatedObjects and add step to generatedLevelSteps. stepIndex[1] => Rotation Speed, stepIndex[2] => StepIndex
            {
                var step = GeneratedObjects.GetChild(i);
                var stepIndexes = step.name.Split('_');
                generatedLevelSteps.Add(new StepReferenceToGame(float.Parse(stepIndexes[1], CultureInfo.InvariantCulture), int.Parse(stepIndexes[2]), step));
                levelEnding += platformSamples[int.Parse(stepIndexes[2])].Height; // Accumulate the levelEnding by summing step heights.

                if (i==0)
                {
                    for (int j=0;j < step.childCount;j++)
                    {
                        var mesh = step.GetChild(j).GetChild(0);
                        var rig = mesh.gameObject.AddComponent<Rigidbody>();
                        rig.useGravity = false;
                        rig.constraints = RigidbodyConstraints.FreezeAll;
                        var col = mesh.gameObject.GetComponent<MeshCollider>();
                        col.enabled = true;
                    }
                }

            } // End of Section 6
            var endingPlatform = Instantiate<GameObject>(endingPlatformSample, GeneratedObjects).transform; // Section 7 - Comment: Instantiate endingPlatform directly, because LevelCreatorScene is opened, and set its position. Also, add it to generatedLevelSteps by copying RotationSpeed of previous step. -1 StepIndex means, this step is a endingPlatform
            endingPlatform.position = new Vector3(0, generatedLevelSteps[generatedLevelSteps.Count - 1].Step.position.y - 0.8f, 0);
            generatedLevelSteps.Add(new StepReferenceToGame(generatedLevelSteps[generatedLevelSteps.Count - 1].RotationSpeed, -1, endingPlatform)); // End of Section 7
            return; // The remaining of the method isn't needed, because this scene is LevelCreator scene.
        }

        var levelObject = Resources.Load<TextAsset>("Levels/" + levelNumber); // Get level text from resources in assets.
        if (levelObject == null) // Section 8 - Comment: If the level doesn't exist, log error message and revert this method.
        {
            if(isLiveAutoGeneratorActive) TriggerLiveAutoGenerator();
            else Debug.LogError("The level that will be loaded is not exist");
            return;
        } // End of Section 8
        var levelContent = levelObject.text; // Read the level text file. 
        levelObject = null; // Release levelObject file. This is for releasing unnecessary memory on RAM.
        var stepsByOrderAsStrings = levelContent.Split('\n'); // Return line contents as array of level content. Each line means one step.

        // Level file content structure => speed / ang / index  /      color      /     color2      / scaleX / scaleZ /   00000
        //                                 float  float  int        int_int_int       int_int_int     float    float   int sequence

        for (int j = 0; j < stepsByOrderAsStrings.Length; j++) // For loop does this for each step.
        {
            var stepsStrings = stepsByOrderAsStrings[j].Split('/'); // Gets step contents as string array (speed, ang, index, etc.)
            float stepRotationSpeed = float.Parse(stepsStrings[0], CultureInfo.InvariantCulture); // Parse string to float. 0 index is speed.
            float stepAngle = float.Parse(stepsStrings[1], CultureInfo.InvariantCulture); // Parse string to float. 1 index is offset angle relative to upper step.
            int stepId = int.Parse(stepsStrings[2]); // Parse string to int. 2 index is step index.
            var stepColorStrings = stepsStrings[3].Split('_'); // Split string to string array by '_' character. This returns color by r,g and b as string array.
            Color stepColor = new Color(float.Parse(stepColorStrings[0], CultureInfo.InvariantCulture) / 255f, float.Parse(stepColorStrings[1], CultureInfo.InvariantCulture) / 255f, float.Parse(stepColorStrings[2], CultureInfo.InvariantCulture) / 255f); // Parsing 0 1 2 indexes to float. This returns each color component in 0-255 range, then divides it with 255 to make it in range 0f-1f.
            var obstacleColorStrings = stepsStrings[4].Split('_'); // Split string to string array by '_' character. This returns color by r,g and b as string array.
            Color obstacleColor = new Color(float.Parse(obstacleColorStrings[0], CultureInfo.InvariantCulture) / 255f, float.Parse(obstacleColorStrings[1], CultureInfo.InvariantCulture) / 255f, float.Parse(obstacleColorStrings[2], CultureInfo.InvariantCulture) / 255f); // Parsing 0 1 2 indexes to float. This returns each color component in 0-255 range, then divides it with 255 to make it in range 0f-1f.
            float scaleX = float.Parse(stepsStrings[5], CultureInfo.InvariantCulture); // Parse string to float. 5 index is scaleX of the step.
            float scaleZ = float.Parse(stepsStrings[6], CultureInfo.InvariantCulture); // Parse string to float. 6 index is scaleZ of the step.
            bool[] isObstacleArray = new bool[stepsStrings[7].Length]; // Create a bool array that's length is equal to int sequence (like 00000 => length is 5) 0 means => normal platform, 1 means => obstacle platform.
            for (int i = 0; i < stepsStrings[7].Length; i++) // Section 9 - Comment: If char is '1' of int sequence, it is an obstacle platform and isObstacleArray[i] value becomes true. For loop does this for each char of the int sequence.
            {
                isObstacleArray[i] = stepsStrings[7][i] == '1';
            }// End of Section 9
            levelEnding += platformSamples[stepId].Height; // Accumulate level ending to calculate level height. This is needed for placing ending platform.
            stepGenerationQueue.Enqueue(new StepGeneration(stepRotationSpeed, stepId, stepAngle, stepColor, obstacleColor, scaleX, scaleZ, isObstacleArray)); // Add this step to stepGenerationQueue with using datas. stepGenerationQueue is used for generating steps on time. If all steps are generated immediately, the game will be slow on mobile phones, even on computers.
        }
        levelIndicatorSource.text = levelNumber.ToString(); // Set levelIndicatorSource text to level number. levelIndicatorSource is left side of the progressing indicator on the top of screen. 
        levelIndicatorTarget.text = Resources.Load<TextAsset>("Levels/" + (levelNumber + 1)) != null || isLiveAutoGeneratorActive ? (levelNumber + 1).ToString() : "END"; // Set levelIndicatorTarget text to level number+1. If there is no level with level number+1, set text to "END".
        GenerateStepsAsBulk(stepGenerationCountOnInitializing); // Generate steps as bulk
    }
    void GenerateStepsAsBulk(int count) // Generates steps as bulk
    {
        float stepYOffset = 0; // Position offset on the y axis.
        for (int i = 0; i < count; i++) // For loop does this for each step
        {
            StepGeneration stepInfo = stepGenerationQueue.Dequeue(); // Get next step from stepGenerationQueue. I used queue for storing steps, because steps can be only adding with 0 index and remove with last index.
            var generatedStep = new GameObject("Step").transform; // Generate the step.
            generatedStep.SetParent(GeneratedObjects); // Set parent of new step as GeneratedObjects. With this way all steps will under the GeneratedObjects gameobject.
            generatedStep.position = new Vector3(0, stepYOffset, 0); // Set step position.
            float lastStepAngle = generatedLevelSteps.Count > 0 ? generatedLevelSteps[generatedLevelSteps.Count - 1].Step.eulerAngles.y : 0; // Get previous step angle to calculate relative rotate angle. If the index is equal to 0, that means, this step is first step of the level and relative angle must be 0.
            generatedStep.eulerAngles = new Vector3(0, lastStepAngle + stepInfo.StepAngle, 0); //  Set rotation of the step by calculating relative rotate angle.
            generatedLevelSteps.Add(new StepReferenceToGame(stepInfo.RotationSpeed, stepInfo.StepIndex, generatedStep)); // Add this step to generatedLevelSteps. generatedLevelSteps is used for rotating and deleting steps with StepUpdate method.
            Step step = platformSamples[stepInfo.StepIndex]; // This is used for getting obstaclePlatformSample, normalPlatformSample, count and height informations.
            for (int j = 0; j < stepInfo.IsObstacleArray.Length; j++) // For loop does this for each platform
            {
                var generatedPlatform = Instantiate<GameObject>(stepInfo.IsObstacleArray[j] ? step.ObstaclePlatformSample : step.NormalPlatformSample, generatedStep).transform; // Generate platform with getting the information from isObstacleArray[j].
                generatedPlatform.name = "Platform_" + (stepInfo.IsObstacleArray[j] ? "1" : "0"); // Name the platform. I added _0 or _1 information end of the name. _0 means => normal platform, _1 means => obstacle platform. _ character is used for splitting two pieces easily with Split method of string.
                generatedPlatform.localEulerAngles = new Vector3(0, (360f / (float)step.Count) * j, 0); // Set rotate of the platform. Rotate difference amount is 360/count.

                if (i == 0)
                {
                    var mesh = generatedPlatform.GetChild(0);
                    var rig = mesh.gameObject.AddComponent<Rigidbody>();
                    rig.useGravity = false;
                    rig.constraints = RigidbodyConstraints.FreezeAll;
                    var col = mesh.gameObject.GetComponent<MeshCollider>();
                    col.enabled = true;
                }

                if (generatedPlatform.GetChild(0).GetComponent<Renderer>().material.shader.name == "Custom/PlatformShader") generatedPlatform.GetChild(0).GetComponent<Renderer>().material.color = stepInfo.IsObstacleArray[j] ? stepInfo.ObstacleColor : stepInfo.StepColor; // If platform sample has Custom/PlatformShader, set its color to stepInfo.obstacleColor or stepInfo.stepColor. With this way, we can use textured platforms with other materials.
            }
            stepYOffset -= step.Height; // Iterate the offset.
            generatedStep.localScale = new Vector3(stepInfo.ScaleX, 1, stepInfo.ScaleZ); // Set step size with information of stepInfo.
            if (stepGenerationQueue.Count == 0) break; // If there is no remaining steps in stepGenerationQueue, break loop. If count is greater than stepGenerationQueue count, this line happens.
        }
    }
    void GenerateStep() // Generates a step
    {
        if (stepGenerationQueue.Count == 0) return; // If there is no remaining steps in stepGenerationQueue, break loop.
        StepGeneration stepInfo = stepGenerationQueue.Dequeue(); // Get next step from stepGenerationQueue. I used queue for storing steps, because steps can be only adding with 0 index and remove with last index.
        var generatedStep = new GameObject("Step").transform; // Generate the step.
        generatedStep.SetParent(GeneratedObjects); // Set parent of new step as GeneratedObjects. With this way all steps will under the GeneratedObjects gameobject.
        generatedStep.position = new Vector3(0, generatedLevelSteps[generatedLevelSteps.Count - 1].Step.position.y - platformSamples[stepInfo.StepIndex].Height, 0); // Set step position. The position of this step is relative to previous step.
        float lastStepAngle = generatedLevelSteps[generatedLevelSteps.Count - 1].Step.eulerAngles.y; // Previous step rotation angle. This will be used for calculating relative rotation angle.
        generatedStep.eulerAngles = new Vector3(0, lastStepAngle + stepInfo.StepAngle, 0); // Set rotation of the step by calculating relative rotate angle.
        generatedLevelSteps.Add(new StepReferenceToGame(stepInfo.RotationSpeed, stepInfo.StepIndex, generatedStep)); // Add this step to generatedLevelSteps. generatedLevelSteps is used for rotating and deleting steps with StepUpdate method.
        for (int j = 0; j < stepInfo.IsObstacleArray.Length; j++) // For loop does this for each platform
        {
            Step step = platformSamples[stepInfo.StepIndex]; // This is used for getting obstaclePlatformSample, normalPlatformSample, count informations.
            var generatedPlatform = Instantiate<GameObject>(stepInfo.IsObstacleArray[j] ? step.ObstaclePlatformSample : step.NormalPlatformSample, generatedStep).transform; // Generate platform with getting the information from isObstacleArray[j].
            generatedPlatform.name = "Platform_" + (stepInfo.IsObstacleArray[j] ? "1" : "0"); // Name the platform. I added _0 or _1 information end of the name. _0 means => normal platform, _1 means => obstacle platform. _ character is used for splitting two pieces easily with Split method of string.
            generatedPlatform.localEulerAngles = new Vector3(0, (360f / (float)step.Count) * j, 0); // Set rotate of the platform. Rotate difference amount is 360/count.
            if (generatedPlatform.GetChild(0).GetComponent<Renderer>().material.shader.name == "Custom/PlatformShader") generatedPlatform.GetChild(0).GetComponent<Renderer>().material.color = stepInfo.IsObstacleArray[j] ? stepInfo.ObstacleColor : stepInfo.StepColor; // If platform sample has Custom/PlatformShader, set its color to stepInfo.obstacleColor or stepInfo.stepColor. With this way, we can use textured platforms with other materials.
        }
        generatedStep.localScale = new Vector3(stepInfo.ScaleX, 1, stepInfo.ScaleZ); // Set step size with information of stepInfo.
        if (stepGenerationQueue.Count == 0) // If there is no remaining steps instantiate ending platform.
        {
            var endingPlatform = Instantiate<GameObject>(endingPlatformSample, GeneratedObjects).transform; // Instantiate ending platform with parent of GeneratedObjects. 
            endingPlatform.position = new Vector3(0, generatedStep.position.y - 0.8f, 0); // Set position of the ending platform. Ending platform half height is 0.8.
            generatedLevelSteps.Add(new StepReferenceToGame(stepInfo.RotationSpeed, -1, endingPlatform)); // Add ending platform to generatedLevelSteps. -1 means, indicating this is an ending platform. Also, copies rotation speed of previous step for the ending platform.
            isEndingPlatformGenerated = true; // Indicates ending platform is generated.
        }
    }
    public void StepUpdate() // Checks steps and rotates
    {
        for (int i = 0; i < generatedLevelSteps.Count; i++) // For loop does this for each generated step in the game scene.
        {
            if (generatedLevelSteps[i] == null || generatedLevelSteps[i].Step.parent != GeneratedObjects) // This condition means if selected step is destroyed or its parent isn't GeneratedObjects.
            {
                generatedLevelSteps.RemoveAt(i); // Just remove step reference from generatedLevelSteps list array.
                i--; // This is needed, because when 1 item is deleted from list, the all indexes after "i" index are decreases by 1.
                continue; // Don't do under codes of this and continue.
            }
            generatedLevelSteps[i].Step.eulerAngles += new Vector3(0, generatedLevelSteps[i].RotationSpeed, 0); // Iterate angle of rotation of each step by the rotationSpeed that comes from generatedLevelSteps information.
        }
    }
    public void PlayerUpdate()
    {
        float normalizedBreakingBonus = bonusBreakingTime / breakingBonusMax; // Normalizes bonusBreakingTime to 0-1 range.
        if (!isGameEnded && generatedLevelSteps.Count == 1) // This if block is only for making game end more precise. Otherwise some errors may encounter.
        {
            // TODO SUCCESS SOUND
            //audioSource.pitch = 1; // Set pitch to 1, because the pitch of the audioSource may be modified.
            //audioSource.PlayOneShot(successSoundClip); // Play success sound clip. Because game is ended with complete or passed all levels action, both are success.
            
            if (levelIndicatorTarget.text != "END") // If right level indicator's text is NOT "END", that means there is more levels after this level.
            {
                endingType = EndingTypes.COMPLETE; // Set ending action.
                GameEnd(); // Call game end to end. (like showing end UI)
            }
            else // If right level indicator's text is "END", that means there is no more levels after this level.
            {
                endingType = EndingTypes.PASSEDALLLEVELS; // The player passed all levels, so ending action is passed all levels.
                GameEnd(); // Call game end to end. (like showing end UI)
            }
        }
        if (!isGameStartMenuDisabled)
        {
            NotControledPlayerByTouching(normalizedBreakingBonus);
            if (isPlayerTouching)
            {
                isGameStartMenuDisabled = true;
                GameStartUI.SetActive(false);
                gameStateSlider.transform.parent.gameObject.SetActive(true);
                scoreValueText.gameObject.SetActive(true);
            }
            return;
        }
        if (isGameEnded || !Input.GetMouseButton(0) || isToleranceEffectActivated) // If game is ended or the player does not touch to screen, do this block.
        {
            NotControledPlayerByTouching(normalizedBreakingBonus); // Call NotControledPlayerByTouching (does free ball bouncing etc.)
        }
        else
        {
            ControledPlayerByTouching(normalizedBreakingBonus); // Call ControledPlayerByTouching (does ball breaking action etc.)
        }
    }
    public void PlayerClickDown()
    {
        isPlayerTouching = true;
    }
    public void PlayerClickUp()
    {
        isPlayerTouching = false;
    }
    public void NotControledPlayerByTouching(float normalizedBreakingBonus) // Control free ball bouncing, splash effect and some animations.
    {
        if (playerTimeCounter == -1) // If playerTimeCounter = -1, that means the player has just released the screen.
        {
            for (float i = 1f; i > 0f; i -= 0.02f) // For loop checks PlayerJumpCurve inversely.
            {
                if (((Player.transform.position.y - 0.20f) - PlayerJumpCurve.Evaluate(i)) < 0.01f) // If distance between player position and PlayerJumpCurve value is lower than 0.01, do this block.
                {
                    playerTimeCounter = i; // The correct location of the player position on the PlayerJumpCurve has found and set the time counter.
                    //GeneratedObjects.position = new Vector3(0, -GeneratedObjects.GetChild(0).localPosition.y, 0); // All steps' position must be set to 0, if the player does not touch the screen. So, assigning first steps' negative Y axis local position to GeneratedObjects Y axis position sets all steps' positions and by this way, first step's global position is set to 0 on Y axis.
                    Player.GetChild(1).GetComponent<ParticleSystem>().Stop(); // Stop the meteor effect. It may emit.
                    cameraShakeAmount = Vector3.zero; // Reset camera shake amount vector for reusing properly.
                    break; // The correct position is found and the loop must be broken.
                }
            }
        }
        if (toleratedStep != null && !isToleranceEffectActivated)
        {
            isToleranceEffectActivated = true;
            StartCoroutine(ToleranceEffect());
        }
        GeneratedObjects.position = new Vector3(0, Mathf.MoveTowards(GeneratedObjects.position.y, -GeneratedObjects.GetChild(0).localPosition.y, 0.2f), 0); // All steps' position must be set to 0, if the player does not touch the screen. So, assigning first steps' negative Y axis local position to GeneratedObjects Y axis position sets all steps' positions and by this way, first step's global position is set to 0 on Y axis.
        Player.GetChild(0).GetComponent<LineRenderer>().enabled = false;
        Player.transform.position = new Vector3(0, PlayerJumpCurve.Evaluate(playerTimeCounter) + 0.20f, playerPositionOnZAxis); // Simple bouncing animation controlled by the curve.
        Player.transform.localScale = new Vector3(Mathf.MoveTowards(Player.transform.localScale.x, PlayerBounceScaleXCurve.Evaluate(playerTimeCounter), 0.05f), PlayerBounceScaleYCurve.Evaluate(playerTimeCounter), 1); // Simple scale animation controlled by the curves. For x scale I used MoveTowards method, because when the player touches the screen, ball expands too much on the X axis, if this method is not used, the scale animation on X axis would be discrete.
        Camera.localRotation = Quaternion.Slerp(Camera.localRotation, Quaternion.Euler(25f, 0f, 0f), 0.3f); // Reset camera rotation. Quaternion is used, because euler angle method has gimbal lock.
        playerTimeCounter += Time.deltaTime * PlayerAnimationSpeed; // Increase time counter.
        if (playerTimeCounter > 1f) // If time counter is greater than 1, that means the animation curve ended and it is needed to reset. I did not use PingPong effect of animation curve, because to make splash and bounce sound effects, a single time operation must be launched and this method achieves that easily.
        {
            playerTimeCounter = 0f; // Reset counter.
            // TODO Bounce sound
            //audioSource.pitch = 1f; // Set pitch to 1. It may be modified.
            //audioSource.PlayOneShot(bounceSoundClip); // Play bounce sound effect, because jump curve has 0 value at 0 time, and this is starting of the bounce.
            var splash = new GameObject("splash").transform; // Generate a game object to use as splash parent.
            splash.parent = generatedLevelSteps.Count != 1 ? generatedLevelSteps[0].Step.GetChild(0).GetChild(0) : generatedLevelSteps[0].Step; // If the first step is normal step, the parent is generatedLevelSteps[0].step.GetChild(0).GetChild(0) (This location is under of the platform mesh object). If the generatedLevelSteps = 1, that means this step is an ending platform and the parent is needed to assign differently. The parent must be generatedLevelSteps[0].step.
            splash.SetAsLastSibling(); // Set the sibling index as last, because other siblings can be used by other codes by using child indexing.
            var splashSprite = splash.gameObject.AddComponent<SpriteRenderer>(); // Add sprite renderer and store it. Because the splash sprite can be renderer by sprite renderer.
            splashSprite.sprite = splashSpriteSamples[Random.Range(0, splashSpriteSamples.Length)]; // Pick a random splash sprite from splashSpriteSamples.
            splashSprite.color = Player.GetChild(0).GetComponent<Renderer>().material.color; // Set splash color to player's material color.
            splash.eulerAngles = new Vector3(90, 0, 0); // Splash effects must be facing up.
            splash.localScale = Vector3.one * splashSpriteScale; // Set scale of the splash sprite.
            splash.position = Player.position; // Set position to Player's position. This is initial position.
            splash.localPosition = new Vector3(splash.localPosition.x, generatedLevelSteps.Count != 1 ? generatedLevelSteps[0].Step.GetChild(0).Find("splashOrigin").localPosition.y : generatedLevelSteps[0].Step.Find("splashOrigin").localPosition.y, splash.localPosition.z); // Get splashOrigin object from platform or ending platform where it is located and set y axis of the splash sprite to splashOrigin y axis position.
            splash.gameObject.AddComponent<SplashDestroyer>(); // Add SplashDestroyer, this destroys splash sprite when it is rotated 1.5 complete turn. (540 degree)
#if UNITY_ANDROID && !UNITY_EDITOR
        if(isVibrationEnabled && !isToleranceEffectActivated)
        {
            // TODO Vibration
            Vibration.Vibrate(3);
        }
#endif
        }
        if (bonusBreakingTime > 0f)
            bonusBreakingTime -= Time.deltaTime * breakingBonusDecreasingSpeed; // If the player does not the screen and bonus breaking time greater than zero, decrease it.
        else
        {
            if (breakBonusWarning.activeSelf) breakBonusWarning.SetActive(false); // If breakBonusWarning is active, deactive it. This is needed because the ball may exit from meteor mode.
            bonusBreakingTime = 0f; // Set bonus breaking time to 0 for exact values.
            isBreakingBonusConsuming = false; // If the ball exited from meteor mode, isBreakingBonusConsuming must be false.
        }

        breakBonus.fillAmount = normalizedBreakingBonus; // Set break bonus indicator filling amount.
        sceneLight.intensity = sceneLightCurve.Evaluate(normalizedBreakingBonus); // Set scene light intensity by evaluating curve.
        breakBonus.color = breakBonusColor.Evaluate(normalizedBreakingBonus); // Set break bonus indicator color by evaluating gradient.
        overrideBottom = backgroundBottomColorTransient.Evaluate(normalizedBreakingBonus); // Meteor background bottom color
        overrideTop = backgroundTopColorTransient.Evaluate(normalizedBreakingBonus); // Meteor background top color
        isNotFirstMovingSteps = false; // It is used for increasing position precise of breaking effect. If the player does not touch the screen, it must be resetted.
        CameraOrigin.eulerAngles = new Vector3(Mathf.LerpAngle(CameraOrigin.eulerAngles.x, 0, 0.15f), 0, 0); // Reset camera origin rotation.
        scoreValueText.transform.localScale = Vector3.MoveTowards(scoreValueText.transform.localScale, Vector3.one, 0.15f); // Reset scoreValueText scale, it may be scaled in meteor mode.
        scoreValueText.color = scoreTextColorGradient.Evaluate(normalizedBreakingBonus); // Reset color of scoreValueText, it may be changed its color in meteor mode.
    }
    public void ControledPlayerByTouching(float normalizedBreakingBonus)
    {
        Player.transform.position = Vector3.MoveTowards(Player.transform.position, new Vector3(0, 0.2f, playerPositionOnZAxis), 0.25f); // The ball must be (0,0.2,-2.4) position to break platforms properly.
        Player.transform.localScale = Vector3.MoveTowards(Player.transform.localScale, playerScaleWhenItIsBreakingPlatforms, 0.1f); // Sets the ball scale.
        playerTimeCounter = -1; // This indicates the player touches to screen and the proper location must be found in NotControledPlayerByTouching method.
        if (Player.transform.position.y == 0.2f) // That means the ball started to break platforms.
        {
            Player.GetChild(0).GetComponent<LineRenderer>().enabled = true;
            if (!isBreakingBonusConsuming) // If the meteor mode is not activated.
            {
                scoreValueText.transform.localScale = Vector3.MoveTowards(scoreValueText.transform.localScale, Vector3.one, 0.1f); // scoreValueText scale is (1,1,1) when meteor mode is not activated.
                scoreValueText.color = scoreTextColorGradient.Evaluate(normalizedBreakingBonus); // Sets scoreValueText color by evaluating gradient.
                if (bonusBreakingTime < breakingBonusMax) bonusBreakingTime += Time.deltaTime * breakingBonusIncreasingSpeed; // Increase bonus breaking time counter.
                else // If bonusBreakingTime is greater than breakingBonusMax and the meteor mode is activated. The effects of meteor effect will be able to appear in next frame. (in else block)
                {
                    bonusBreakingTime = breakingBonusMax; // Sets bonusBreakingTime to breakingBonusMax, this is needed for exact values.
                    isBreakingBonusConsuming = true; // Meteor mode is activated.
                    breakBonusWarning.SetActive(true); // Warning indicator is activated and it has play on awake animation component so it will play fading in-out animation once it is activated.
                    Player.GetChild(1).GetComponent<ParticleSystem>().Play(); // Meteor effect is started.
                }
            }
            else // If the meteor mode is activated. Meteor effects can appear here.
            {
                AttemptToGenerateMeteor(); // Attemp to generate background meteor figures.
                CameraOrigin.eulerAngles = new Vector3(Mathf.LerpAngle(CameraOrigin.eulerAngles.x, CameraRotateDegreeWhenPlayerIsMeteor, 0.15f), 0, 0); // Set camera origin rotation.
                scoreValueText.transform.localScale = Vector3.MoveTowards(scoreValueText.transform.localScale, Vector3.one * scoreScaleWhenMeteorModeActive, 0.11f); // Set scoreValueText scale.

                if (cameraShakeAmount == Vector3.zero) // If camera shake amount is 0, this effect is for meteor mode camera shaking. The logic is; set camera rotation in one frame by randomly and recover camera the rotation in next frame.
                {
                    cameraShakeAmount = Random.insideUnitSphere * CameraShakeMultiplier; // Set shake amount randomly.
                    Camera.eulerAngles += cameraShakeAmount; // Apply shake amount.
                }
                else // If cameraShakeAmount is not (0,0,0), the camera is just shaked.
                {
                    Camera.eulerAngles -= cameraShakeAmount; // Recover camera rotation. 
                    cameraShakeAmount = Vector3.zero; // Reset the shake amount.
                }

                if (bonusBreakingTime > 0f) bonusBreakingTime -= Time.deltaTime * breakingBonusDecreasingSpeedWhenMeteor; // Decrease bonus breaking time, because the meteor mode consumes bonus breaking time.
                else // If bonusBreakingTime < 0
                {
                    bonusBreakingTime = 0; // Set bonus breaking time to 0 for exact values.
                    isBreakingBonusConsuming = false; // The meteor mode is deactivated.
                    breakBonusWarning.SetActive(false); // The meteor mode is deactivated.
                    Player.GetChild(1).GetComponent<ParticleSystem>().Stop(); // Stop meteor effect of the ball.
                }
            }
            breakBonus.fillAmount = normalizedBreakingBonus; // Set break bonus indicator filling amount.
            breakBonus.color = breakBonusColor.Evaluate(normalizedBreakingBonus); // Set break bonus indicator color by evalueating gradient.
            float offsetTime = (isBreakingBonusConsuming ? 0.6f : 0f); // If meteor mode is activated (isBreakingBonusConsuming = true), set 0.6 offset time.
            sceneLight.intensity = sceneLightCurve.Evaluate(normalizedBreakingBonus + offsetTime); // Set scene light by evalueating curve.
            overrideBottom = backgroundBottomColorTransient.Evaluate(normalizedBreakingBonus + offsetTime); // Set background bottom color 
            overrideTop = backgroundTopColorTransient.Evaluate(normalizedBreakingBonus + offsetTime); // Set background top color 
            PlatformBreakAndChecking(); // Check and break platform if needed.
        }
    }
    void AttemptToGenerateMeteor() // Generates meteor if random time is elapsed.
    {
        if (meteorTimeCounter > meteorRandomValue) // If time counter greater than meteorRandomValue, generate meteor. 
        {
            meteorRandomValue = Random.Range(0f, MeteorGenerateTimeCounterRandomMax); // Set new random value.
            meteorTimeCounter = 0; // Reset time counter
            var t = Instantiate<GameObject>(MeteorSample, GarbageObjects).transform; // Generate meteor object and set its parent to GarbageObjects.
            t.position = new Vector3(Random.Range(3f, 5f) * (Random.value > 0.5f ? 1 : -1), 10f, Random.Range(5f, 7f) * (Random.value > 0.6f ? 1f : -1f)); // Set meteor position randomly.
            t.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(MeteorXAxisVelocityMin, MeteorXAxisVelocityMax) * (t.position.x < 0 ? 1 : -1), Random.Range(MeteorYAxisVelocityMin, MeteorYAxisVelocityMax), 0); // Set meteor velocity randomly.
            Destroy(t.gameObject, MeteorDestroyTime); // Destroy meteor after MeteorDestroyTime time.
        }
        meteorTimeCounter += Time.deltaTime; // Increase time counter.
    }
    void PlatformBreakAndChecking() // Break platform and checks, also triggers break effect for steps.
    {
        var touchedStepsDictionary = Player.GetComponent<PlayerController>().TouchingPlatforms; // Touched steps information that comes from PlayerController component on Player. This information is collected by using Physic Engine. 
        var touchedStepKeys = touchedStepsDictionary.Keys.ToArray<Transform>(); // touchedStepsDictionary is stored as Dictionary, so this line gets all keys as transform array.
        bool isObstacleHit = false; // This holds the Player is touching any obstacle platform. False is the initial value.
        for (int i = 0; i < touchedStepsDictionary.Count; i++) // For loop does this for each touch steps.
        {
            if (touchedStepKeys[i] == null) // If touched step key is null, that means the touched step is destroyed before. (The player can touch two or more platform of one step, this block prevents it)
            {
                touchedStepsDictionary.Remove(touchedStepKeys[i]); // Remove it
                i--; // When remove an item from dictionary, it's length is changed and after i index, the all indexes decreased by one.
                continue; // This platform must not concern to delete or not.
            }
            if (touchedStepKeys[i].tag == "WillBeDeletedStep") continue; // If a step marked as "WillBeDeletedStep", it will be destroyed and deleted next frame due touching of the player another platform of this step and skip this iteration with continue.
            touchedStepsDictionary.TryGetValue(touchedStepKeys[i], out isObstacleHit); // Gets value of key-value pair of touchedStepsDictionary. (Key is step transform, value is the platform is an obstacle or not)
            if (isToleranceEnabled && isObstacleHit && !isBreakingBonusConsuming && touchedStepKeys[i].position.y < hittingToleranceAmount)
            {
                toleratedStep = touchedStepKeys[i];
                isObstacleHit = false;
                isNotFirstMovingSteps = true;
                continue;
            }
            PlatformBreakEffect(touchedStepKeys[i]); // Triggers platform break effect for the step.
            touchedStepKeys[i].tag = "WillBeDeletedStep"; // Marks as WillBeDeletedStep the step.
            if (!isObstacleHit || isBreakingBonusConsuming)
            {
                scoreValue += isBreakingBonusConsuming ? scoreIncreaseAmountWhenPlayerIsMeteor : scoreIncreaseAmount; // Increase the score with scoreIncreaseAmountWhenPlayerIsMeteor or scoreIncreaseAmount. If the meteor mode active, breaking steps give more point logically.
                scoreValueText.text = ((int)scoreValue).ToString(); // Set scoreValueText text with updated scoreValue.
            }
            touchedStepsDictionary.Remove(touchedStepKeys[i]); // Remove the step from touchedStepsDictionary, because this step will be deleted soon.
            GenerateStep(); // One step is deleted from top and one step must be generated from bottom if there is more step in stepGenerationQueue queue.
            if (!isNotFirstMovingSteps) isNotFirstMovingSteps = true; // If a step is deleted, that means the all steps must be moving up.
        }
        if (isNotFirstMovingSteps) // Steps can move up and can play sound effects etc.
        {
            if (isObstacleHit && !isBreakingBonusConsuming) // Did the player hit any obstacle and is meteor mode deactivated?
            {
                // TODO BREAK SOUND
                
                endingType = EndingTypes.GAMEOVER; // The player hit to an obstacle and the game has finished.
                GameEnd(); // Call game end to end. (like showing end UI)
            }
            if (isEndingPlatformGenerated && generatedLevelSteps.Last<StepReferenceToGame>().Step.position.y < -5f) Camera.position += new Vector3(0, 0.132f * 0.75f * 0.25f, 0);
            if (generatedLevelSteps.Count != 1) GeneratedObjects.position += new Vector3(0, 0.132f * 1.5f, 0);
            gameStateSlider.value = gameStateSlider.minValue + (GeneratedObjects.position.y / levelEnding) * (gameStateSlider.maxValue - gameStateSlider.minValue);
            if (!isEndingPlatformGenerated && CenterCylinder.transform.position.y < 0.5f) CenterCylinder.transform.position += new Vector3(0, 0.132f * 1.5f, 0);
            if (isEndingPlatformGenerated && generatedLevelSteps.Count < 25) CenterCylinder.position = new Vector3(0, CenterCylinder.localScale.y + generatedLevelSteps.Last<StepReferenceToGame>().Step.position.y, 0);
            if (generatedLevelSteps.Count == 1) // generatedLevelSteps count is equal to 1, that means there is no normal step, there is only ending platform. End game finished with complete or passed all levels action.
            {
                // TODO SUCCESS SOUND
                if (levelIndicatorTarget.text != "END") // If right level indicator's text is NOT "END", that means there is more levels after this level.
                {
                    endingType = EndingTypes.COMPLETE; // Set ending action.
                    GameEnd(); // Call game end to end. (like showing end UI)
                }
                else // If right level indicator's text is "END", that means there is no more levels after this level.
                {
                    endingType = EndingTypes.PASSEDALLLEVELS; // The player passed all levels, so ending action is passed all levels.
                    GameEnd(); // Call game end to end. (like showing end UI)
                }
            }
        }
    }
    void PlatformBreakEffect(Transform step) // Breaks a step with effect.
    {
        if (isVibrationEnabled)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // TODO Vibration
            Vibration.Vibrate(15);
#endif
        }
        step.parent = GarbageObjects; // This step is a garbage for now, it is useless and it will be destroyed and deleted soon. So its parent is set as GarbageObjects.

        if (GeneratedObjects.GetChild(0) && GeneratedObjects.childCount > 1)
        {
            var newFirstStep = GeneratedObjects.GetChild(0);
            for (int i = 0; i < newFirstStep.childCount; i++)
            {
                var mesh = newFirstStep.GetChild(i).GetChild(0);
                var rig = mesh.gameObject.AddComponent<Rigidbody>();
                rig.useGravity = false;
                rig.constraints = RigidbodyConstraints.FreezeAll;
                var col = mesh.gameObject.GetComponent<MeshCollider>();
                col.enabled = true;
            }
        }

        // TOOO SOUNDS
        //audioSource.pitch = 1 + (isBreakingBonusConsuming ? 0 : bonusBreakingTime / breakingBonusMax) * stepBreakingSoundEffectPitchMultiplier; // Set pitch of audioSource according to bonusBreakingTime value, with this way, while bonusBreakingTime is increasing, the breaking sound effect becomes thinner. But isBreakingBonusConsuming is true, the meteor sound effect must not be pinched, so if isBreakingBonusConsuming is true, this value becomes 1 + 0.
        //audioSource.PlayOneShot(isBreakingBonusConsuming ? meteorBreakSoundClip : breakSoundClip); // Play meteor or break sound effect according to isBreakingBonusConsuming.
        
        Destroy(step.gameObject, stepDestroyTime); // Destroy this step after stepDestroyTime time.

        for (int i = 0; i < step.childCount; i++) // For loop does this for each platform of this step.
        {
            var rig = step.GetChild(i).GetChild(0).gameObject.GetComponent<Rigidbody>(); // Gets rigidbody of the platform mesh object.
            Destroy(step.GetChild(i).GetChild(0).GetComponent<MeshCollider>());
            rig.constraints = RigidbodyConstraints.None; // No restriction about moving or rotating rigidbody in breaking process.
            rig.angularVelocity = Vector3.zero; // Set platform to no rotating. This is needed because steps can hit each other.
            rig.velocity = Vector3.zero; // Set platform to no moving. This is needed because steps can hit each other.
            step.GetChild(i).GetChild(0).GetComponent<MeshCollider>().enabled = false; // Disables the mesh collider of the platform mesh object.
            rig.AddForce((step.GetChild(i).GetChild(0).eulerAngles.y < 95 || step.GetChild(i).GetChild(0).eulerAngles.y > 265f) ? -800f * Mathf.Clamp(2f / bonusBreakingTime * stepXAxisForceBreakingTimeMultiplier, 0.2f, 2) * stepXAxisForceMultiplier : 800f * Mathf.Clamp(2f / bonusBreakingTime * stepXAxisForceBreakingTimeMultiplier, 0.2f, 2) * stepXAxisForceMultiplier, 3500f * stepYAxisForceMultiplier, 0f); // Applies the force to platform to make break effect. If the platform is on the left side, the force is applied to left with random magnitude, otherwise the force is applied to right with random magnitude.
            rig.AddRelativeTorque(Vector3.right * -300f * Random.Range(0.2f, 10f)); // Applies torque to platform with random magnitude, this is needed for making the break effect more realistic.
        }
    }
    void GameEnd() // Shows end game UI, updates total score and score values on the end game UI and resets some visual components.
    {
        isGameEnded = true; // If there is no steps else endingPlatform, game ended.

        if (secondChanceCoroutine != null)
        {
            StopCoroutine(secondChanceCoroutine); // If second chance counter is working, stop it.
        }

        bool isSecondChanceAsked = false;

        if (isSecondChanceActive && secondChanceLimitCounter > 0 && generatedLevelSteps.Count > 1 && AdMobManager.Instance.IsRewardedAdLoaded)
        {
            isSecondChanceAsked = true;
            secondChanceCoroutine = StartCoroutine(SecondChanceCounter());
            secondChanceLimitCounter--;
        }
        gameEndUI.GameEndUIParent.gameObject.SetActive(true); // Activates game end UI.
        gameEndUI.PassedInfoText.gameObject.SetActive(false); // This line fixes second chance bug.
        gameEndUI.PassAllLevelsInfoText.gameObject.SetActive(false); // This line fixes second chance bug.
        gameEndUI.GameOverInfoText.gameObject.SetActive(false); // This line fixes second chance bug.
        breakBonus.fillAmount = 0; // Set breakBonus filling amount to 0, this is needed for when game ended, the break bonus indicator is not shown on background.
        breakBonusWarning.SetActive(false); // Disable warning indicator.
        gameEndUI.CurrentScoreValue.text = scoreValueText.text; // Update current score text on game end UI.
        int newScoreIfpassed = (int)scoreValue + int.Parse(gameEndUI.TotalScoreValue.text); // If the level has been passed, the new score is needed to be calculated.
        switch (endingType)
        {
            case EndingTypes.COMPLETE: // On complete level action.
                gameEndUI.TouchToContinue.gameObject.SetActive(true); // Activates touchToContinue text.
                gameEndUI.PassedInfoText.gameObject.SetActive(true); // Activates passedInfotext text.
                gameEndUI.TotalScoreValue.text = newScoreIfpassed.ToString(); // Updates totalScoreValue text with new total score.
                if (SceneManager.sceneCount == 1)
                {
                    PlayerPrefs.SetInt("passed_levels", currentLevel + 1); // Stores new passed levels number to "passed_levels" key.
                    PlayerPrefs.SetInt("total_score", newScoreIfpassed); // Stores new total score to "total_score" key.
                    PlayerPrefs.Save(); // Saves all changes on the PlayerPrefs.
                }
                break;
            case EndingTypes.PASSEDALLLEVELS: // On passed all levels action.
                gameEndUI.TouchToRestartText.gameObject.SetActive(true); // Activates touchToRestartText text.
                gameEndUI.PassAllLevelsInfoText.gameObject.SetActive(true); // Activates passAllLevelsInfoText text.
                gameEndUI.TotalScoreValue.text = newScoreIfpassed.ToString(); // Updates totalScoreValue text with new total score.
                if (SceneManager.sceneCount == 1)
                {
                    PlayerPrefs.SetInt("total_score", newScoreIfpassed); // Stores new total score to "total_score" key.
                    PlayerPrefs.Save(); // Saves all changes on the PlayerPrefs.
                }
                break;
            case EndingTypes.GAMEOVER: // On gameover level action.
                Destroy(Player.gameObject); // Destroy the player, because it hit to obstacle.
                GeneratedObjects.position = new Vector3(0,-GeneratedObjects.GetChild(0).localPosition.y,0); // All steps' position must be set to 0, if the player does not touch the screen. So, assigning first steps' negative Y axis local position to GeneratedObjects Y axis position sets all steps' positions and by this way, first step's global position is set to 0 on Y axis.
                CrackedPlayer = Instantiate<GameObject>(CrackedPlayerSample).transform; // Generate cracked player. Cracked player has multiple separated meshes, so its meshes can move independently.
                CrackedPlayer.position = Player.position; // Set cracked player position as player position.
                if (isVibrationEnabled)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                // TODO Vibration
                //Vibration.Vibrate(new long[] { 600, 300, 600, 300 }, 1);
                StartCoroutine(CancelVibration(3f));
#elif UNITY_IOS && !UNITY_EDITOR
                Handheld.Vibrate();
#endif
                }
                for (int i = 0; i < CrackedPlayer.childCount; i++) // For loop does this for each cracked player mesh inside cracked player.
                {
                    var rig = CrackedPlayer.GetChild(i).GetComponent<Rigidbody>(); // Get rigidbody of piece of cracked player.
                    rig.AddForce(CrackedPlayer.GetChild(i).localPosition * Random.Range(350f, 550f)); // Apply force with random magnitude to this rigidbody. CrackedPlayer.GetChild(i).localPosition means the force vector direction is to outside, so this can be used as explosion effect.
                    rig.AddTorque(Random.onUnitSphere * Random.Range(50f, 150f)); // Apply some torque to this rigidbody with random magnitude, with this way, the effect will be more realistic.
                }
                if (!isSecondChanceAsked) gameEndUI.TouchToRestartText.gameObject.SetActive(true); // Activates touchToRestartText text.
                gameEndUI.GameOverInfoText.gameObject.SetActive(true); // Activates gameOverInfoText text.
                break;
        }
        if (endingType == EndingTypes.COMPLETE || endingType == EndingTypes.PASSEDALLLEVELS)
        {
            if (isVibrationEnabled)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // TODO Vibration
                // Vibration.Vibrate(new long[] { 500, 100, 150, 400, 100, 150, 400, 100 }, 1);
                StartCoroutine(CancelVibration(4));
#elif UNITY_IOS && !UNITY_EDITOR
                Handheld.Vibrate();
#endif
            }
            var endingPlatform = generatedLevelSteps[0].Step;
            endingPlatform.Find("ConfettiGeneratorLeft").GetComponent<ParticleSystem>().Play();
            endingPlatform.Find("ConfettiGeneratorRight").GetComponent<ParticleSystem>().Play();
        }
    }
    
    private IEnumerator SecondChanceCounter()
    {
        int remainingTime = secondChanceTimeCounterMax - 1;
    
        gameEndUI.SecondChanceCounterText.text = secondChanceTimeCounterMax.ToString();
        gameEndUI.SecondChanceCounterText.gameObject.SetActive(true);
        gameEndUI.SecondChanceInfoText.gameObject.SetActive(true);
        gameEndUI.SecondChanceWatchAdText.gameObject.SetActive(true);
        gameEndUI.TotalScoreValue.gameObject.SetActive(false);
        gameEndUI.TotalScoreText.gameObject.SetActive(false);
        gameEndUI.CurrentScoreText.gameObject.SetActive(false);
        gameEndUI.CurrentScoreValue.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        while (remainingTime >= 0)
        {
            gameEndUI.SecondChanceAnimator.Play("SecondChanceDecreaseAnim");
            
            yield return new WaitForSeconds(0.33f);
        
            gameEndUI.SecondChanceCounterText.text = remainingTime.ToString();
            remainingTime--;

            yield return new WaitForSeconds(0.66f);
        }

        gameEndUI.SecondChanceCounterText.gameObject.SetActive(false);
        gameEndUI.SecondChanceInfoText.gameObject.SetActive(false);
        gameEndUI.SecondChanceWatchAdText.gameObject.SetActive(false);
        gameEndUI.TotalScoreValue.gameObject.SetActive(true);
        gameEndUI.TotalScoreText.gameObject.SetActive(true);
        gameEndUI.CurrentScoreText.gameObject.SetActive(true);
        gameEndUI.CurrentScoreValue.gameObject.SetActive(true);
        gameEndUI.TouchToRestartText.gameObject.SetActive(true);

        secondChanceCoroutine = null;
    }
    
    IEnumerator ToleranceEffect()
    {
        float currentScale = toleratedStep.localScale.x;
        float animatingScale = currentScale;
        
        if (isVibrationEnabled)
        {
            Vibration.VibratePeek();
        }  
        
        while (currentScale * 1.5f - animatingScale > 0.1f)
        {
            if(toleratedStep == null)
            {
                Vibration.CancelAndroid();
                isToleranceEffectActivated = false;
                yield break;
            }
            animatingScale = Mathf.MoveTowards(animatingScale,currentScale * 1.5f,0.07f);
            toleratedStep.localScale = new Vector3(animatingScale,1,animatingScale);
            yield return null;
        }
        
        while (animatingScale - currentScale > 0.06f)
        {
            if (toleratedStep == null)
            {
                Vibration.CancelAndroid();
                isToleranceEffectActivated = false;
                yield break;
            }
            animatingScale = Mathf.MoveTowards(animatingScale, currentScale, 0.04f);
            toleratedStep.localScale = new Vector3(animatingScale, 1, animatingScale);
            yield return null;
        }
        
        toleratedStep.localScale = new Vector3(currentScale, 1, currentScale);

        toleratedStep = null;
        isToleranceEffectActivated = false;
    }
    
    public void RestartGame()
    {
        bool isSecondChanceWanted = secondChanceCoroutine != null && generatedLevelSteps.Count > 1;
        
        if (secondChanceCoroutine != null)
        {
            StopCoroutine(secondChanceCoroutine);
            secondChanceCoroutine = null;
        }

        gameEndUI.GameEndUIParent.gameObject.SetActive(false);
        gameEndUI.SecondChanceCounterText.gameObject.SetActive(false);
        gameEndUI.SecondChanceInfoText.gameObject.SetActive(false);
        gameEndUI.SecondChanceWatchAdText.gameObject.SetActive(false);
        gameEndUI.TotalScoreValue.gameObject.SetActive(true);
        gameEndUI.TotalScoreText.gameObject.SetActive(true);
        gameEndUI.CurrentScoreText.gameObject.SetActive(true);
        gameEndUI.CurrentScoreValue.gameObject.SetActive(true);

        if (isSecondChanceWanted)
        {
            AdMobManager.Instance.ShowInterstitialAd();
            
            if (CrackedPlayer != null)
            {
                Destroy(CrackedPlayer.gameObject);
            }
            
            CrackedPlayer = null;
            Player = Instantiate(PlayerSample).transform;
            isGameEnded = false;
            endingType = EndingTypes.NONE;
        }

        switch (endingType)
        {
            case EndingTypes.COMPLETE:
            {
                gameEndUI.TouchToContinue.gameObject.SetActive(false);
                gameEndUI.PassedInfoText.gameObject.SetActive(false);
                
                if (!isSecondChanceWanted)
                {
                    LoadLevel(currentLevel + 1);
                }
                break;
            }
            case EndingTypes.PASSEDALLLEVELS:
            {
                gameEndUI.TouchToRestartText.gameObject.SetActive(false);
                gameEndUI.PassAllLevelsInfoText.gameObject.SetActive(false);
                
                if (!isSecondChanceWanted)
                {
                    LoadLevel(currentLevel);
                }
                break;
            }
            case EndingTypes.GAMEOVER:
            {
                gameEndUI.TouchToRestartText.gameObject.SetActive(false);
                gameEndUI.GameOverInfoText.gameObject.SetActive(false);
                
                if (!isSecondChanceWanted)
                {
                    LoadLevel(currentLevel);
                }
                break;
            }
        }
    }
    
    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            Vibration.CancelAndroid();
        }
    }

    private void TriggerLiveAutoGenerator()
    {
        Random.State backupRandom = Random.state;
        Random.InitState(autoGeneratorSeed + currentLevel);
        Random.InitState(Random.Range(-currentLevel * 12345, currentLevel * 12345));

        int stepCount = (int)Random.Range(
            totalStepMinCurve.Evaluate((float)currentLevel / maxHardnessLevelNumber) * 100,
            totalStepMaxCurve.Evaluate((float)currentLevel / maxHardnessLevelNumber) * 100 + 1);

        string levelContent = String.Empty;
        List<AutoGeneratorColorPair> colorPairs = AutoGeneratorColorPairs;
        AutoGeneratorColorPair colorPair = colorPairs[Random.Range(0, colorPairs.Count)];
        int patternIndex = Random.Range(0, platformSamples.Length);
        Step pattern = platformSamples[patternIndex];
        float snapAngle = 360f / (float)pattern.Count;
        float lastAngle = Random.Range(0f, 360f);
        int[] stepIsObstacleArray = new int[pattern.Count];
        float speed = (Random.Range(0f,
                           autoGeneratorHardnessCurve.Evaluate((float)currentLevel / (maxHardnessLevelNumber + 1))) +
                       1.5f) *
                      (Random.Range(0f, 1f) < 0.5f ? -1f : 1f);
        
        AnimationCurve scaleCurve = AutoGenerateScales[Random.Range(0, AutoGenerateScales.Count)];

        bool isAnyPlatformNormal = false;
        
        for (int j = 0; j < pattern.Count; j++)
        {
            stepIsObstacleArray[j] = Random.Range(0, 2);
            
            if (stepIsObstacleArray[j] == 0)
            {
                isAnyPlatformNormal = true;
            }
        }

        if (!isAnyPlatformNormal)
        {
            stepIsObstacleArray[Random.Range(0, pattern.Count)] = 0;
        }

        for (int stepIndex = 0; stepIndex < stepCount; stepIndex++)
        {
            bool isTrapped = IsTrapOccurred(currentLevel);
            float angleDifference = speed * speedToAngle;

            if (isTrapped)
            {
                isAnyPlatformNormal = false;
                
                for (int j = 0; j < pattern.Count; j++)
                {
                    stepIsObstacleArray[j] = Random.Range(0, 2);
                    
                    if (stepIsObstacleArray[j] == 0)
                    {
                        isAnyPlatformNormal = true;
                    }
                }

                if (!isAnyPlatformNormal)
                {
                    stepIsObstacleArray[Random.Range(0, pattern.Count)] = 0;
                }

                angleDifference += (Random.Range(0f, 1f) < 0.5f ? -1f : 1f) * snapAngle *
                                   Random.Range(1, pattern.Count);
                if (angleDifference >= 360)
                {
                    angleDifference = 360 - angleDifference;
                }
                else if (angleDifference < 0)
                {
                    angleDifference += 360;
                }
            }

            Color normalPlatformColor =
                colorPair.NormalPlatformGradientColor.Evaluate(stepIndex / (float)stepCount);
            Color obstaclePlatformColor =
                colorPair.ObstaclePlatformGradientColor.Evaluate(stepIndex / (float)stepCount);

            int normalRed = (int)(normalPlatformColor.r * 255f);
            int normalGreen = (int)(normalPlatformColor.g * 255f);
            int normalBlue = (int)(normalPlatformColor.b * 255f);

            int obstacleRed = (int)(obstaclePlatformColor.r * 255f);
            int obstacleGreen = (int)(obstaclePlatformColor.g * 255f);
            int obstacleBlue = (int)(obstaclePlatformColor.b * 255f);

            float scaleValue = scaleCurve.Evaluate((float)stepIndex / stepCount);
            string stepContent = speed.ToString(CultureInfo.InvariantCulture) + "/" +
                                 (stepIndex == 0 ? lastAngle : angleDifference).ToString(CultureInfo.InvariantCulture) + "/" +
                                 patternIndex.ToString(CultureInfo.InvariantCulture) + "/" +
                                 normalRed.ToString(CultureInfo.InvariantCulture) + "_" +
                                 normalGreen.ToString(CultureInfo.InvariantCulture) + "_" +
                                 normalBlue.ToString(CultureInfo.InvariantCulture) + "/" +
                                 obstacleRed.ToString(CultureInfo.InvariantCulture) + "_" +
                                 obstacleGreen.ToString(CultureInfo.InvariantCulture) + "_" +
                                 obstacleBlue.ToString(CultureInfo.InvariantCulture) + "/" +
                                 scaleValue.ToString(CultureInfo.InvariantCulture) + "/" +
                                 scaleValue.ToString(CultureInfo.InvariantCulture) + "/";

            for (int index = 0; index < pattern.Count; index++)
            {
                stepContent += stepIsObstacleArray[index].ToString(CultureInfo.InvariantCulture);
            }

            stepContent += "/";
            
            if (stepIndex != stepCount - 1)
            {
                stepContent += "\n";
            }

            if (IsTrapOccurredForSpeed(currentLevel))
            {
                speed = (Random.Range(0f,
                            autoGeneratorHardnessCurve.Evaluate(((float)currentLevel) / (maxHardnessLevelNumber + 1))) +
                        1.5f) *
                        (Random.Range(0f, 1f) < 0.5f ? -1f : 1f);
            }

            levelContent += stepContent;
        }

        Random.state = backupRandom;

        string[] stepsByOrderAsStrings = levelContent.Split('\n');

        for (int j = 0; j < stepsByOrderAsStrings.Length; j++)
        {
            string[] stepsStrings = stepsByOrderAsStrings[j].Split('/');
            float stepRotationSpeed = float.Parse(stepsStrings[0], CultureInfo.InvariantCulture);
            float stepAngle = float.Parse(stepsStrings[1], CultureInfo.InvariantCulture);
            int stepId = int.Parse(stepsStrings[2]);

            string[] stepColorStrings = stepsStrings[3].Split('_');
            Color stepColor = new(float.Parse(stepColorStrings[0], CultureInfo.InvariantCulture) / 255f,
                float.Parse(stepColorStrings[1], CultureInfo.InvariantCulture) / 255f,
                float.Parse(stepColorStrings[2], CultureInfo.InvariantCulture) / 255f);

            string[] obstacleColorStrings = stepsStrings[4].Split('_');
            Color obstacleColor = new(float.Parse(obstacleColorStrings[0], CultureInfo.InvariantCulture) / 255f,
                float.Parse(obstacleColorStrings[1], CultureInfo.InvariantCulture) / 255f,
                float.Parse(obstacleColorStrings[2], CultureInfo.InvariantCulture) / 255f);

            float scaleX = float.Parse(stepsStrings[5], CultureInfo.InvariantCulture);
            float scaleZ = float.Parse(stepsStrings[6], CultureInfo.InvariantCulture);
            bool[] isObstacleArray = new bool[stepsStrings[7].Length];

            for (int i = 0; i < stepsStrings[7].Length; i++)
            {
                isObstacleArray[i] = stepsStrings[7][i] == '1';
            }

            levelEnding += platformSamples[stepId].Height;
            stepGenerationQueue.Enqueue(new StepGeneration(stepRotationSpeed, stepId, stepAngle, stepColor,
                obstacleColor, scaleX, scaleZ, isObstacleArray));
        }

        levelIndicatorSource.text = currentLevel.ToString();
        levelIndicatorTarget.text = (currentLevel + 1).ToString();
        GenerateStepsAsBulk(stepGenerationCountOnInitializing);
    }
        
    private bool IsTrapOccurred(float levelIndex, float occurrenceDecreaser)
    {
        float hardness = autoGeneratorHardnessCurve.Evaluate(levelIndex / (maxHardnessLevelNumber + 1));
        return Random.Range(0f, 1f + occurrenceDecreaser) < hardness;
    }

    private bool IsTrapOccurred(float levelIndex) => IsTrapOccurred(levelIndex, trapOccurenceDecreaser);
    private bool IsTrapOccurredForSpeed(float levelIndex) => IsTrapOccurred(levelIndex, speedTrapOccurenceDecreaser);
}