using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TestControllerReading : MonoBehaviour
{
    //Basically, the problem I'm having is that when you move the controller around, down isn't always down.  I know why, but
    //I don't know what to do about it.
    //https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir

    //So, I'm not a huge fan of how Unity is doing this.  But basically for the XR stuff you have to create a device list,
    //then assing stuff to your device list, then you check things in your device list.  I'm sure there's a reason for all
    //this, but I don't understand it.
    private List<InputDevice> leftHandedControllers = new List<InputDevice>();
    private List<InputDevice> rightHandedControllers = new List<InputDevice>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //This will check for the angle of the controller.
        Quaternion deviceAngle;
        Quaternion downAngle = new Quaternion(-1.0f, 0.0f, 0.0f, 0.0f);

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandedControllers);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandedControllers);
        if (rightHandedControllers.Count > 0)
        {
            //So, I know we just checked for firing, but now we're going to check for reload.
            if (rightHandedControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out deviceAngle))
            {
                //Try to check for reload?
                Debug.Log("Try to check for right reload.");
                Debug.Log(deviceAngle.ToString());
                float angle = Quaternion.Angle(deviceAngle, downAngle);
                Debug.Log("New angle is: " + angle.ToString());

            }
        }
        //if (leftHandedControllers.Count > 0)
        //{
        //    //So, I know we just checked for firing, but now we're going to check for reload.
        //    if (leftHandedControllers[0].TryGetFeatureValue(CommonUsages.deviceRotation, out deviceAngle))
        //    {
        //        //Try to check for reload?
        //        Debug.Log("Try to check for left reload.");
        //        Debug.Log(deviceAngle.ToString());
        //    }
        //}
    }
}