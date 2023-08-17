// **********************************************************************************
// Special thanks to this guy : https://github.com/beinteractive/Uween for this pure 
// piece of code... UWEEN : finally a library of tweening simple to understand...
// **********************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;

public class CreepyCatTest : MonoBehaviour {

    public GameObject myObject;
    public bool loopAnim = false;
    public float moveDistance = 2.0f;
    public float moveTimeA = 2.0f;
    public float moveTimeB = 2.0f;

    private Vector3 originalPos;
//    private Quaternion originalRot;
  
    // Enum for ease
    public enum MyEnum{
        One, Two, Three
    }

    public MyEnum fooBar;


	// Use this for initialization
	void Start () {
        originalPos = myObject.transform.localPosition;
     //   originalRot = myObject.transform.localRotation;
	}

    void EndAnim()
    {
        if (loopAnim == false){
            
            TweenY.Add(myObject,moveTimeB, originalPos.y).EaseOutElastic() ;
        //    TweenSXYZ.Add(myObject, moveTimeB, 1).EaseOutElastic();
        }else{
            TweenY.Add(myObject, moveTimeB, originalPos.y).EaseOutElastic().Then(StartAnim);
        //    TweenSXYZ.Add(myObject, moveTimeB, 1).EaseOutElastic().Then(StartAnim);      
        }
    }

    void StartAnim()
    {
        TweenY.Add(myObject, moveTimeA, originalPos.y - moveDistance).EaseOutElastic().Then(EndAnim);
       // TweenSXYZ.Add(myObject, 2f, 3).EaseOutElastic().Then(EndAnim);

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            StartAnim();
        }
	}
}



//using UnityEngine;
//using Uween;

//public class Example : MonoBehaviour
//{
//    GameObject image;

//    void Awake()
//    {
//        image = transform.Find("Image").gameObject;
//    }

//    void Start()
//    {
//        animate();
//    }

//    void animate()
//    {
//        Debug.Log("Animate");

//        // Move X - One Shot
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f));

//        // Move X - Repeat (Complete callback)
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f)).Then(animate);

//        // Move X - Repeat - Ease In Out Sine
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f)).EaseInOutSine().Then(animate);

//        // Move X - Repeat - Ease In Out Sine - With Delay
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Move X - Repeat - Ease In Out Sine - With Delay - With First Position
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f)).EaseInOutSine().Delay(0.5f).From(0f).Then(animate);

//        // Move X - Repeat - Ease In Out Sine - From Specified Position to Current Position
//        // TweenX.Add(image, 1f).From(Random.Range(Screen.width / -2f, Screen.width / 2f)).EaseInOutSine().Then(animate);

//        // Move X - Repeat - Ease In Out Sine - To Position from Current Position + Specified Position
//        // TweenX.Add(image, 1f, 90f).FromRelative(-30f).EaseInOutSine().Then(animate);

//        // Move X - Repeat - Ease In Out Sine - To Position that Current Position + Specified Position
//        // TweenX.Add(image, 1f, 60f).Relative().EaseInOutSine().Then(animate);

//        // Move X - Repeat - Ease In Out Sine - From Specified Position + Current Position to Current Position
//        // TweenX.Add(image, 1f).FromRelative(60f).EaseInOutSine().Then(animate);

//        // Move X - Repeat - Ease In Out Back - With Delay
//        // TweenX.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f)).EaseInOutBack().Delay(0.5f).Then(animate);

//        // Move Y - Repeat - Ease In Out Sine - With Delay
//        // TweenY.Add(image, 1f, Random.Range(Screen.height / -2f, Screen.height / 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Move X,Y - Repeat - Ease In Out Sine - With Delay
//        // TweenXY.Add(image, 1f, Random.Range(Screen.width / -2f, Screen.width / 2f),  Random.Range(Screen.height / -2f, Screen.height / 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Scale X - Repeat - Ease In Out Sine - With Delay
//        // TweenSX.Add(image, 1f, Random.Range(0.5f, 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Scale Y - Repeat - Ease In Out Sine - With Delay
//        // TweenSY.Add(image, 1f, Random.Range(0.5f, 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Scale X,Y - Repeat - Ease In Out Sine - With Delay
//        // TweenSXY.Add(image, 1f, Random.Range(0.5f, 2f), Random.Range(0.5f, 2f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Rotation Z - Repeat - Ease In Out Sine - With Delay
//        // TweenRZ.Add(image, 1f, Random.Range(-360f, 360f)).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Change Alpha - Repeat - Ease In Out Sine - With Delay - With First Value
//        // TweenA.Add(image, 1f, 0f).EaseInOutSine().Delay(0.5f).From(1f).Then(animate);

//        // Change Color - Repeat - Ease In Out Sine - With Delay - With First Value
//        // Color[] colors = {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};
//        // Color c = colors[Random.Range(0, colors.Length - 1)];
//        // TweenC.Add(image, 1f, c).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Change Color - Repeat - Ease In Out Sine - With Delay - With First Value
//        // Color[] colors = {Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};
//        // Color c = colors[Random.Range(0, colors.Length - 1)];
//        // c.a = Random.Range(0f, 1f);
//        // TweenCA.Add(image, 1f, c).EaseInOutSine().Delay(0.5f).Then(animate);

//        // Change Fill Amount - Repeat - Ease In Out Sine - With Delay - With First Value
//        // UnityEngine.UI.Image im = image.GetComponent<UnityEngine.UI.Image>();
//        // im.type = UnityEngine.UI.Image.Type.Filled;
//        // im.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
//        // TweenFillAmount.Add(image, 1f, 1f).EaseInOutSine().Delay(0.5f).From(0f).Then(animate);

//        // Simply Wait & Callback
//        // TweenNull.Add(image, 1f).Then(animate);
//    }
//}


