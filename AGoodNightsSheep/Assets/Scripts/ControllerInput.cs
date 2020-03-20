using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

public class ControllerInput : MonoBehaviour
{
    [Header("Set in Inspector: Projectile Prefab")]
    [SerializeField]
    private GameObject rightProjectilePrefab;
    [SerializeField]
    private GameObject leftProjectilePrefab;
    [Header("Set in Inspector: Gun Transforms")]
    [SerializeField]
    private Transform rightGunBarrel;
    [SerializeField]
    private Transform leftGunBarrel;
    [SerializeField]
    private Text rightGunDisplay;
    [SerializeField]
    private Text leftGunDisplay;
    [SerializeField]
    private AudioSource rightGunAudioSource;
    [SerializeField]
    private AudioSource leftGunAudioSource;

    private GameManager gameManager;

    [Header("Gun Reload Rates")]
    public float rightReloadRate = 2.0f;
    public float leftReloadRate = 2.0f;

    [Header("Gun Clip Sizes")]
    //Just for fun, I'm thinking about adding a clip size to guns that enforces a reload.  Wouldn't that be interesting?
    private int rightMagazineSize = 8;
    private int leftMagazineSize = 8;

    [Header("Controller Haptic Feedback Values")]
    //Alright, so these next variables will be used for haptic feedback.  Sexy!
    private uint rightChannel = 0;
    private float rightAmplitude = 0.75f;
    private float rightDuration = 0.25f;
    private float rightAmplitudeEmpty = 0.1f;
    private float rightReloadAmplitude = 0.25f;
    private float rightDurationEmpty = 0.1f;
    private uint leftChannel = 0;
    private float leftAmplitude = 0.75f;
    private float leftDuration = 0.5f;
    private float leftAmplitudeEmpty = 0.1f;
    private float leftReloadAmplitude = 0.25f;
    private float leftDurationEmpty = 0.1f;

    //These will track how many shots have been fired so that we can check for a reload.
    private int rightShotsFired = 0;
    private int leftShotsFired = 0;

    //These values are used to track the time of the last shot.  Used to keep player from shooting too fast.
    protected float rightLastFire;
    protected float leftLastFire;

    //So, I'm not a huge fan of how Unity is doing this.  But basically for the XR stuff you have to create a device list,
    //then assing stuff to your device list, then you check things in your device list.  I'm sure there's a reason for all
    //this, but I don't understand it.
    private List<InputDevice> leftHandedControllers = new List<InputDevice>();
    private List<InputDevice> rightHandedControllers = new List<InputDevice>();

    //These booleans are going to check for the trigger to be pressed down since we need to make sure they have been released.
    private bool leftTriggerDown = false;
    private bool rightTriggerDown = false;

    // Use this for initialization
    void Start()
    {
        //Get a reference to the game manager.
        gameManager = GameManager.singleton;

        //Start listening for some events.
        gameManager.NewRoundStart += StartRound;

        StartRound();
    }

    /// <summary>
    /// In the update, we are going to check for the player to be shooting.  With the new XR update, I will actually have to check for the button
    /// to be released as well as pressed.  Didn't have to before, but whatever.
    /// </summary>
    void Update()
    {
        //Step 1:  Make sure the game is still going.  Don't want to be shooting if the game is over.
        if (!gameManager.isGameOver)
        {
            //So, this is where I start having problems with Unity's implementation of the XR devices.  You don't assign them like normal.
            //You actually pass in your list, and it puts everything in your list for you.  You just call the function and give your empty
            //list as a parameter.  Weird.  Also, and most importantly, devices can come and go as they please.  So you constantly have to
            //check for them.  If one goes missing, you can't assume it is still there.  Also, it doesn't get a value reassigned to it.  So
            //its good because if a battery dies or something things don't break.  But it's kind of a pain to have to constantly search for them.
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandedControllers);
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandedControllers);

            //This will be used to check for the trigger being pressed.
            bool triggerBool = false;

            //This will check for the angle of the controller.
            Quaternion deviceAngle;

            //So, everything I'm reading says this should work.  But it isn't working, and I have no idea why.  Is it becasue the headset has to be on?
            //Yes.  Fucking yes is the fucking answer to that stupid fucking question three hours later.

            //Now then, make sure we have left handed devices.
            if (leftHandedControllers.Count > 0)
            {
                if (leftHandedControllers[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerBool))
                {
                    //Debug.Log("Left trigger value: " + triggerBool);
                    ProjectileFireLeft(triggerBool);
                }

                //So, I know we just checked for fireing, but now we're going to check for reload.
                if (leftHandedControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out deviceAngle))
                {
                    //Try to check for reload?
                    //Debug.Log("Try to check for left reload.");
                    LeftReloadCheck(deviceAngle);
                }
            }
            //Make sure we have right handed devices first.
            if (rightHandedControllers.Count > 0)
            {
                if (rightHandedControllers[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerBool))
                {
                    //Debug.Log("Right trigger value: " + triggerBool);
                    ProjectileFireRight(triggerBool);
                }

                //So, I know we just checked for firing, but now we're going to check for reload.
                if (rightHandedControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out deviceAngle))
                {
                    //Try to check for reload?
                    //Debug.Log("Try to check for right reload.");
                    RightReloadCheck(deviceAngle);
                }
            }
        }
    }

    /// <summary>
    /// This is going to start off a round.  First, it will reset some values for shooting and the like.
    /// Then, it will get some values from the game manager and set those to the different guns.  Sweet!
    /// </summary>
    private void StartRound()
    {
        //Debug.Log("Starting a new round happening for ControllerInput.");

        //Set some values so that shots can be fired as soon as the game starts.
        rightLastFire = Time.time - rightReloadRate;
        leftLastFire = Time.time - leftReloadRate;
        rightShotsFired = 0;
        leftShotsFired = 0;

        //Now, set all the values with our new values before the next round starts!
        rightMagazineSize = gameManager.rightMagazineSize;
        leftMagazineSize = gameManager.leftMagazineSize;
        rightChannel = gameManager.rightChannel;
        rightAmplitude = gameManager.rightAmplitude;
        rightDuration = gameManager.rightDuration;
        rightAmplitudeEmpty = gameManager.rightAmplitudeEmpty;
        rightReloadAmplitude = gameManager.rightReloadAmplitude;
        rightDurationEmpty = gameManager.rightDurationEmpty;
        leftChannel = gameManager.leftChannel;
        leftAmplitude = gameManager.leftAmplitude;
        leftDuration = gameManager.leftDuration;
        leftAmplitudeEmpty = gameManager.leftAmplitudeEmpty;
        leftReloadAmplitude = gameManager.leftReloadAmplitude;
        leftDurationEmpty = gameManager.leftDurationEmpty;

    }

    #region "Right Gun"

    /// <summary>
    /// See about firing the right pistol.  So, with the new XR stuff, we constantly get the value of the trigger.
    /// We will need to track that value to deteremine when it has been first pressed, and also when it has been released.
    /// This will ensure that players only get one shot per trigger press.
    /// </summary>
    /// <param name="val"></param>
    private void ProjectileFireRight(bool val)
    {
        //First, we'll check to see if the trigger had not been pressed before, and is being pressed now.
        if (!rightTriggerDown && val)
        {
            //This means the player intends to shoot.  So set rightTriggerDown first.
            rightTriggerDown = true;

            //Here, check to see if enough time has passed to allow the player to shoot.
            //We moved reload off to another function, so we also need to make sure the 
            //Clip isn't empty.  I realize I'm kind of doing the clip thing backwards, but whatever.
            if (Time.time - rightLastFire >= rightReloadRate && rightShotsFired < rightMagazineSize)
            {

                //Now, generate the projectile rotation and such.
                Quaternion bananaRotation = rightGunBarrel.transform.rotation;

                //Finally, instantiate the projectile
                GameObject projectile = Instantiate(rightProjectilePrefab, rightGunBarrel.position, bananaRotation);

                //Play the sound of shooting.
                rightGunAudioSource.Play();

                //Increase our right shot count.
                rightShotsFired++;

                //Test to see if we can send haptic feedback.  That'd be sweet.
                TryHapticFeedback(rightHandedControllers[0], rightChannel, rightAmplitude, rightDuration);

                //See how many are left in the magazine.
                int mag = rightMagazineSize - rightShotsFired;

                //Update the display.
                if (mag > 0)
                {

                    rightGunDisplay.text = string.Format("{0}", mag);
                }
                else
                {
                    rightGunDisplay.text = string.Format("{0}", "EMPTY");
                }
            }
            else
            {
                //Clip empty.  Play the empty clip feedback.
                TryHapticFeedback(rightHandedControllers[0], rightChannel, rightAmplitudeEmpty, rightDurationEmpty);
            }
        }
        //Next, check to see if the right trigger was down, and it isn't now.
        else if (rightTriggerDown && !val)
        {
            //Reset the right trigger down.  That's it.
            rightTriggerDown = false;
        }
    }

    /// <summary>
    /// Here, we will check to see about reloading the right gun.  
    /// </summary>
    /// <param name="angle"></param>
    private void RightReloadCheck(Quaternion angle)
    {
        //Because of stuff, I need to make sure we are not at full ammo first.  Cool.
        if(rightShotsFired > 0)
        {
            //Debug.Log("Right controller angle: " + angle.ToString());
            //Great, we have a reason to reload.  Check to see if we do.
            if(angle.x <= -.55)
            {
                //Debug.Log("Reload");
                rightGunDisplay.text = "RELOADING...";

                //We need to reload.  Set the time.
                rightLastFire = Time.time;

                //Also, we'll go ahead and reset the clip size.
                rightShotsFired = 0;

                //Lastly, we'll do haptic feedback for this
                TryHapticFeedback(rightHandedControllers[0], rightChannel, rightReloadAmplitude, rightReloadRate);

                //Now, update the status for the gun.
                StartCoroutine("FinishRightReload");
            }
        }
        //Debug.Log("Right controller angle is: " + angle.ToString());
    }
    #endregion

    #region "Left Gun"

    /// <summary>
    /// See about firing the left pistol.  So, with the new XR stuff, we constantly get the value of the trigger.
    /// We will need to track that value to deteremine when it has been first pressed, and also when it has been released.
    /// This will ensure that players only get one shot per trigger press.
    /// </summary>
    /// <param name="val"></param>
    private void ProjectileFireLeft(bool val)
    {
        //First, we'll check to see if the trigger had not been pressed before, and is being pressed now.
        if (!leftTriggerDown && val)
        {
            //This means the player intends to shoot.  So set leftTriggerDown first.
            leftTriggerDown = true;

            //Here, check to see if enough time has passed to allow the player to shoot.
            if (Time.time - leftLastFire >= leftReloadRate && leftShotsFired < leftMagazineSize)
            {
                //Great!  We can shoot!

                //Now, generate the projectile rotation and such.
                Quaternion bananaRotation = leftGunBarrel.transform.rotation;

                //Finally, instantiate the projectile
                GameObject projectile = Instantiate(leftProjectilePrefab, leftGunBarrel.position, bananaRotation);

                //Play the sound of shooting.
                leftGunAudioSource.Play();

                //Increase our left shot count.
                leftShotsFired++;

                //Test to see if we can send haptic feedback.  That'd be sweet.
                TryHapticFeedback(leftHandedControllers[0], leftChannel, leftAmplitude, leftDuration);

                //See how many are left in the magazine.
                int mag = leftMagazineSize - leftShotsFired;

                //Update the display.
                if (mag > 0)
                {

                    leftGunDisplay.text = string.Format("{0}", mag);
                }
                else
                {
                    leftGunDisplay.text = string.Format("{0}", "EMPTY");
                }
            }
            else
            {
                //Clip empty.  Play the empty clip feedback.
                TryHapticFeedback(leftHandedControllers[0], leftChannel, leftAmplitudeEmpty, leftDurationEmpty);
            }
        }
        //Next, check to see if the left trigger was down, and it isn't now.
        else if (leftTriggerDown && !val)
        {
            //Reset the left trigger down.  That's it.
            leftTriggerDown = false;
        }
    }

    /// <summary>
    /// Here, we will check to see about reloading the left gun.  
    /// </summary>
    /// <param name="angle"></param>
    private void LeftReloadCheck(Quaternion angle)
    {
        //Because of stuff, I need to make sure we are not at full ammo first.  Cool.
        if (leftShotsFired > 0)
        {
            //Great, we have a reason to reload.  Check to see if we do.
            if (angle.x <= -.55)
            {
                //Debug.Log("Reload");
                leftGunDisplay.text = "RELOADING...";

                //We need to reload.  Set the time.
                leftLastFire = Time.time;

                //Also, we'll go ahead and reset the clip size.
                leftShotsFired = 0;

                //Lastly, we'll do haptic feedback for this
                TryHapticFeedback(leftHandedControllers[0], leftChannel, leftReloadAmplitude, leftReloadRate);

                //Now, update the status for the gun.
                StartCoroutine("FinishLeftReload");
            }
        }
        //Debug.Log("Right controller angle is: " + angle.ToString());
    }
    #endregion

    /// <summary>
    /// This function is going to create haptic feedback.  Yeah boyee!!!
    /// </summary>
    /// <param name="device">InputDevice that we want to try and provide haptic feedback to.</param>
    /// <param name="channel">I don't know much about this, but it's probably zero.</param>
    /// <param name="amplitude">Value of the amplitude: 0.0 - 1.0</param>
    /// <param name="duration">How long in seconds you want the feedback to last.</param>
    private void TryHapticFeedback(InputDevice device, uint channel, float amplitude, float duration)
    {
        //So, apparently we don't actually need to do all this try stuff, but I guess we should?
        //First, get a variable to get capabilities back into.
        HapticCapabilities capabilities;

        //Try to get the capabilities of the device.
        if (device.TryGetHapticCapabilities(out capabilities))
        {
            //We got device capabilities back.  Check to see if they support impulse.
            if (capabilities.supportsImpulse)
            {
                //Impulse supported! Let's do it!
                device.SendHapticImpulse(channel, amplitude, duration);
            }
        }
    }

    #region "Coroutines"
    /// <summary>
    /// This will just finish the left reload and update the status indicator to show how much ammo is left.
    /// </summary>
    /// <returns></returns>
    IEnumerator FinishRightReload()
    {
        yield return new WaitForSeconds(rightReloadRate);
        rightGunDisplay.text = string.Format("{0}", rightMagazineSize);
    }

    /// <summary>
    /// This will just finish the left reload and update the status indicator to show how much ammo is left.
    /// </summary>
    /// <returns></returns>
    IEnumerator FinishLeftReload()
    {
        yield return new WaitForSeconds(leftReloadRate);
        leftGunDisplay.text = string.Format("{0}", leftMagazineSize);
    }
    #endregion

    #region "Notes"
    /// <summary>
    /// Litterally just a place to keep notes while I do things.
    /// </summary>
    private void funTesting()
    {
        //Get the input devices from the right hand.
        InputDevice device = rightHandedControllers[0];

        //Create a list of features to populate.
        var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();

        //Try get the the features from the device, and then output the data.  Yay.
        if (device.TryGetFeatureUsages(inputFeatures))
        {
            foreach (var feature in inputFeatures)
            {
                if (feature.type == typeof(bool))
                {
                    bool featureValue;
                    if (device.TryGetFeatureValue(feature.As<bool>(), out featureValue))
                    {
                        Debug.Log(string.Format("Bool feature {0}'s value is {1}", feature.name, featureValue.ToString()));
                    }
                }
                else if (feature.type == typeof(float))
                {
                    float featureFloat;
                    if (device.TryGetFeatureValue(feature.As<float>(), out featureFloat))
                    {
                        Debug.Log(string.Format("Float feature {0}'s value is {1}", feature.name, featureFloat.ToString()));
                    }
                }
                else
                {
                    Debug.Log(string.Format("Feature {0} of type {1} has value: ", feature.name, feature.type));
                }

            }
        }
    }
    #endregion
}
