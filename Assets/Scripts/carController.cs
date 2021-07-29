using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent (typeof(InputManager))]


public class carController : MonoBehaviour
{

	internal enum driveType
	{
		FrontWheelDrive,
		RearWheelDrive,
		AllWheelDrive
	}
	[SerializeField] private driveType carDriveType;	
	
	// objects
	private GameObject wheelObject;
	private GameObject colliders;
	private GameObject centerOfMass;
	public AnimationCurve gearRatios;
	public AnimationCurve torqueCurve;
	private GameObject[] wheelMesh = new GameObject[4] ;
	private GameObject[] collidersList = new GameObject[4];
	[HideInInspector]public WheelCollider[] wheelColliders = new WheelCollider[4];

	public float[] gearSpeeds; 
	private InputManager im;
	private Rigidbody car;
	private WheelFrictionCurve  forwardFriction,sidewaysFriction;
	private carManager CarManager;

	//car values
	public float topSpeed;
	public float  maxGearChangeRPM, minGearChangeRPM;

	[HideInInspector]public float engineRPM;
	[HideInInspector]public int gearNum = 1;                           
	[HideInInspector]public float currSpeed;
	[HideInInspector]public float fwdInput, backInput, horizontalInput;
	[HideInInspector]public float traction;
	[HideInInspector]public float slipLimit;

		// NOT so garbage
	private float turningRate = 4;			//rotating car;
	private Quaternion _targetRotationx , _targetRotationz;
	public  float frictionMultiplier = 3f;
	private float handBrakeFriction  = 0.05f;
	private float downForce;
	private float topSpeedDrag ,idleDrag = 0.05f;
	public  float runningDrag =0.02f;
	private float idleRPM = 1000;
	private float finalDriveRatio1 = 4.8f, finalDriveRatio2 = 3.9f;
	private float speedMultiplier = 3.6f; 			// used to calculate kph
	private bool checkSpeeds = true;
	private float smoothTime = 0.1f;				//smoth time
	private float turnCheckSense = 10000;
	private float acc = 0;
	private float throttle;
	private float brakingPower = 1000;	
	private float totalTorque;
	private float outputTorque;
	private float wheelRPM;
	private float turnAngle;
	private float maxReverseSpeed = 30;
	private float reverseDrag = 0.6f;
	private float local_finalDrive;
	private float iRPM;
	private float thrAgg = 0.8f;
	private float radius; 
	private float tempo;   	//wheelSpin pointer 
	public float handBrakeFrictionMultiplier = 2;
		//AI

	private void Start() {
		if(SceneManager.GetActiveScene().name  == "awake")return;
		car = GetComponent<Rigidbody> ();
		im = GetComponent<InputManager> ();	
		CarManager = GetComponent<carManager>();
		CarManager.enabled = true; 
		getObjects();	
		StartCoroutine(numeratori());

	}

	void FixedUpdate (){
		if(SceneManager.GetActiveScene().name  == "awake")return;

		fwdInput = im.forward;
		backInput = im.backward ;

		adjustTraction();
		manageCar();
		calcTorque();
		moveCar ();
		ackermanSteering ();
		animateWheels ();
		brakeCar();
		checkWheelSpin();
	
	}
	
	void manageCar(){
		CarManager.currentRPM = engineRPM;
		CarManager.currentGear = gearNum;
		CarManager.currentSpeed = currSpeed;
		CarManager.pitchValue = (engineRPM <= maxGearChangeRPM + 100)?0.2f + engineRPM / (maxGearChangeRPM):1.2f;
		if(im.breaking)CarManager.brakeLightsON(); else CarManager.brakeLightsOFF();
	
	}
		
	void getObjects(){
		
		colliders = gameObject.transform.Find("Colliders").gameObject;
		collidersList[0] = colliders.transform.Find("front Left").gameObject;
		collidersList[1] = colliders.transform.Find("front Right").gameObject;
		collidersList[2] = colliders.transform.Find("rear Left").gameObject;
		collidersList[3] = colliders.transform.Find("rear Right").gameObject;

		wheelObject = gameObject.transform.Find("wheels").gameObject;
		wheelMesh[0] = wheelObject.transform.Find("front Left").gameObject;
		wheelMesh[1] = wheelObject.transform.Find("front Right").gameObject;
		wheelMesh[2] = wheelObject.transform.Find("rear Left").gameObject;
		wheelMesh[3] = wheelObject.transform.Find("rear Right").gameObject;
		//colliders

		wheelColliders[0] = collidersList[0].GetComponent<WheelCollider>();
		wheelColliders[1] = collidersList[1].GetComponent<WheelCollider>();
		wheelColliders[2] = collidersList[2].GetComponent<WheelCollider>();
		wheelColliders[3] = collidersList[3].GetComponent<WheelCollider>();

		centerOfMass = gameObject.transform.Find("center of mass").gameObject;
		car.centerOfMass = centerOfMass.transform.localPosition;
		topSpeedDrag = runningDrag +0.005f;

	}

	public void applyBooster(float amount){
		float R =Mathf.Abs((currSpeed /(topSpeed * 2)) * 15000);
		if(fwdInput != 0 ){
		car.AddForce(transform.forward * (1 + (currSpeed / topSpeed)* 5000));
		car.AddForce(-transform.right * amount * R *2);
		}
	}

	void moveCar ()
	{
		if (carDriveType == driveType.AllWheelDrive) {
			outputTorque = totalTorque / 4;

		wheelColliders [0].motorTorque = wheelColliders [1].motorTorque = wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = outputTorque;


		} else if (carDriveType == driveType.FrontWheelDrive) {
			outputTorque = totalTorque / 2;
			
			wheelColliders [0].motorTorque = outputTorque;
			wheelColliders [1].motorTorque = outputTorque;
			
		} else {
			outputTorque = totalTorque / 2;
			wheelColliders [2].motorTorque = outputTorque;
			wheelColliders [3].motorTorque = outputTorque;
		}
		
	}

	void ackermanSteering ()
	{
		_targetRotationx = transform.rotation;
		_targetRotationx.x = 0;
		_targetRotationz = transform.rotation;
		_targetRotationz.z = 0;

		if(transform.rotation.x > .15f || transform.rotation.x < -.15f){
	     	transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotationx, Time.deltaTime * turningRate);	
		}
		if(transform.rotation.z > .10f || transform.rotation.z < -.10f){
	     	transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotationz, Time.deltaTime * turningRate);
		}

		horizontalInput = im.Horizontal;

        //acerman steering formula
		//steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
        
        if (horizontalInput > 0 ) {
				//rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontalInput;
        } else if (horizontalInput < 0 ) {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * horizontalInput;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
			//transform.Rotate(Vector3.up * steerHelping);

        } else {
            wheelColliders[0].steerAngle =0;
            wheelColliders[1].steerAngle =0;
        }

	}

	void brakeCar ()
	{
		for (int i = 0; i < 4; i++) {
			wheelColliders [i].brakeTorque = (backInput < 0 )?brakingPower: 0;
			if(backInput < 0 && fwdInput ==0)car.AddForce(-transform.forward * 1000);
			if(currSpeed < 0 && backInput == 0 )wheelColliders[i].brakeTorque = brakingPower * 18000;
			else wheelColliders[i].brakeTorque = 0;
		}


	}

	void adjustFinalDrive ()
	{
		if (gearNum == 1 || gearNum == 4 || gearNum == 5) {
			local_finalDrive = finalDriveRatio1;
		} else {
			local_finalDrive = finalDriveRatio2;
		}
	}

	void calcTorque (){	
		
		acc = (gearNum == 1) ? Mathf.MoveTowards (0, 1 * fwdInput, thrAgg) : 1;
		throttle = checkForTraction(fwdInput);
		shiftGear ();
		getEngineRPM ();

		totalTorque = torqueCurve.Evaluate (engineRPM) * (gearRatios.Evaluate (gearNum)) * local_finalDrive * throttle * acc;

		if (engineRPM >= maxGearChangeRPM){
			totalTorque = 0;
			engineRPM = maxGearChangeRPM;
			}
	}

	void shiftGear ()
	{
		if ((gearNum < gearRatios.length - 1 && engineRPM >= maxGearChangeRPM || (gearNum == 0 && (fwdInput > 0 || backInput < 0))) && !isFlying () && checkGearSpeed ()) {
			gearNum++;
		}
		if (gearNum > 1 && engineRPM <= minGearChangeRPM)
			gearNum--;
		if (checkStandStill () && backInput < 0)
			gearNum = -1;
		if (gearNum == -1 && checkStandStill () && fwdInput > 0)
			gearNum = 1;
	}

	bool checkGearSpeed ()
	{
		if (gearNum != -1) {
			if (checkSpeeds) {
				return currSpeed >= gearSpeeds [gearNum - 1];
			} else
				return true;
		} else
			return false;
	}

	void idlingRPM ()
	{
		iRPM = (gearNum > 1) ? 0 : idleRPM;
	}

	void getEngineRPM ()
	{
		idlingRPM ();
		getWheelRPM ();
		float velocity = 0.0f;
		engineRPM = Mathf.SmoothDamp (engineRPM, iRPM + (Mathf.Abs (wheelRPM) * local_finalDrive * gearRatios.Evaluate (gearNum)), ref velocity, smoothTime);
		currSpeed = car.velocity.magnitude *3.6f;
	}

	void getWheelRPM ()
	{
		float sum = 0;
		int c = 0;
		for (int i = 0; i < 4; i++) {
			sum += wheelColliders [i].rpm;
			c++;
		}
		wheelRPM = (c != 0) ? sum / c : 0;
	}

	void animateWheels ()
	{
		Vector3 wheelPosition = Vector3.zero;
		Quaternion wheelRotation = Quaternion.identity;

		for (int i = 0; i < 4; i++) {
			wheelColliders [i].GetWorldPose (out wheelPosition, out wheelRotation);
			wheelMesh [i].transform.position = wheelPosition;
			wheelMesh [i].transform.rotation = wheelRotation;
		}
	}

	void adjustDrag ()
	{


		if (currSpeed >= topSpeed)
			car.drag = topSpeedDrag;
		else if (outputTorque == 0)
			car.drag = idleDrag;
		else if (currSpeed >= maxReverseSpeed && gearNum == -1 && wheelRPM <= 0)
			car.drag = reverseDrag;
		else {
			car.drag = runningDrag;
		}
	}

	void addDownForce(){
		downForce = currSpeed /2 ;
		car.AddForce (-transform.up * downForce * car.velocity.magnitude); //  Down force

	}

	bool isFlying ()
	{
		if (!wheelColliders [0].isGrounded && !wheelColliders [1].isGrounded && !wheelColliders [2].isGrounded && !wheelColliders [3].isGrounded) {
			return true;
		} else
			return false;
	}

	bool checkStandStill ()
	{
		if (currSpeed == 0) {
			return true;
		} else {
			return false;
		}
	}

	void adjustTraction(){
		if(!im.handBrake){
			forwardFriction = wheelColliders[0].forwardFriction;
			sidewaysFriction = wheelColliders[0].sidewaysFriction;

			forwardFriction.extremumValue = forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
			sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;

			for (int i = 0; i < 4; i++) {
				wheelColliders [i].forwardFriction = forwardFriction;
				wheelColliders [i].sidewaysFriction = sidewaysFriction;

			}
		}
		
		else if(im.handBrake){
		sidewaysFriction = wheelColliders[0].sidewaysFriction;
		forwardFriction = wheelColliders[0].forwardFriction;

		//sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = handBrakeFriction;
		//forwardFriction.extremumValue = forwardFriction.asymptoteValue = handBrakeFriction;
		
		float velocity = 0;
		sidewaysFriction.extremumValue =sidewaysFriction.asymptoteValue= Mathf.SmoothDamp(sidewaysFriction.asymptoteValue,handBrakeFriction,ref velocity ,0.05f * Time.deltaTime);
		forwardFriction.extremumValue = forwardFriction.asymptoteValue = Mathf.SmoothDamp(forwardFriction.asymptoteValue,handBrakeFriction,ref velocity ,0.05f * Time.deltaTime);

		for (int i = 2; i < 4; i++) {
			wheelColliders [i].sidewaysFriction = sidewaysFriction;
			wheelColliders [i].forwardFriction = forwardFriction;
		}

		sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =  1.5f;
		forwardFriction.extremumValue = forwardFriction.asymptoteValue =  1.5f;

		for (int i = 0; i < 2; i++) {
			wheelColliders [i].sidewaysFriction = sidewaysFriction;
			wheelColliders [i].forwardFriction = forwardFriction;
		}
		}
		
		CarManager.spinning = (im.handBrake)?true : false;

	}

	void checkWheelSpin(){

		float blind = 0.28f;

		if(Input.GetKey(KeyCode.LeftShift))
			car.AddForce(transform.forward * 15000);
		if(im.handBrake){
			for(int i = 0;i<4 ;i++){
				WheelHit wheelHit;
				wheelColliders[i].GetGroundHit(out wheelHit);
				if(wheelHit.sidewaysSlip > blind || wheelHit.sidewaysSlip < -blind){
					applyBooster(wheelHit.sidewaysSlip);
				}
			}

		}

		for(int i = 2;i<4 ;i++){
            WheelHit wheelHit;

            wheelColliders[i].GetGroundHit(out wheelHit);

			if(wheelHit.sidewaysSlip < 0 )	
				tempo = (1 + -im.Horizontal) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier) ;
				if(tempo < 0.5) tempo = 0.5f;
			if(wheelHit.sidewaysSlip > 0 )	
				tempo = (1 + im.Horizontal )* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);
				if(tempo < 0.5) tempo = 0.5f;
			if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
				//handBrakeFriction = tempo * 3;
				float velocity = 0;
				handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
				}
			else

				handBrakeFriction = tempo;
		}

		

	}

	IEnumerator numeratori(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			adjustFinalDrive();
			adjustDrag();
			addDownForce();
			radius =(currSpeed > 50)? 6 + (currSpeed / topSpeed) * 40 : 6 ;

		}
	}

	float checkForTraction(float EE){

		for(int i = 2;i<4 ;i++){
			WheelHit wheelHit;
			wheelColliders[i].GetGroundHit(out wheelHit);
			if(wheelHit.forwardSlip > .12f || wheelHit.forwardSlip < -.12f ){
				return EE -= wheelHit.forwardSlip;
			}
			if((wheelHit.sidewaysSlip > .3f || wheelHit.sidewaysSlip < -0.3f) && !im.handBrake){
				return EE -= Mathf.Abs(wheelHit.sidewaysSlip);
			}
		}
		return fwdInput;
	}

}