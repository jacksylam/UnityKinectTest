using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class PlayerController : MonoBehaviour {

   public float speed;

   private Rigidbody rb;
   private int count;
   public Text countText;
   public Text wintText;


   public GameObject BodySourceManager;

   private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
   private BodySourceManager _BodyManager;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        wintText.text = "";
	}
	
	// Update is called once per frame
    void Update() {
        //float moveHorizontal = Input.GetAxis("Horizontal");
        // float moveVertical = Input.GetAxis("Vertical");

        // Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        Vector3 movement = Vector3.zero;

        if (BodySourceManager == null) {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null) {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null) {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data) {
            if (body == null) {
                continue;
            }

            if (body.IsTracked) {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds) {
            if (!trackedIds.Contains(trackingId)) {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        int nearestBodyID = -1;
        for (int i = 0; i < data.Length; i++ ) {
            Kinect.Body body = data[i];
            if (body.IsTracked) {
                if (nearestBodyID == -1) {
                    nearestBodyID = i;
                }
                else {
                    Kinect.Joint head = body.Joints[Kinect.JointType.Head];
                    if (head.Position.Z < data[nearestBodyID].Joints[Kinect.JointType.Head].Position.Z) {
                        nearestBodyID = i;
                    }
                }
            }       
        }

        //Debug.Log(nearestBodyID);

        if (nearestBodyID == -1) {
            return;
        }
        Kinect.Body _body = data[nearestBodyID];
        Kinect.Joint rightHand = _body.Joints[Kinect.JointType.HandRight];
        Kinect.Joint leftHand = _body.Joints[Kinect.JointType.HandLeft];

        Kinect.Joint spineMid = _body.Joints[Kinect.JointType.SpineMid];
        Kinect.Joint spineBase = _body.Joints[Kinect.JointType.SpineBase];

        if (rightHand.Position.X > spineMid.Position.X) {
            movement = Vector3.right;
        }
        else if (rightHand.Position.X < spineMid.Position.X) {
            movement = Vector3.left;
        }
        else {
            movement = Vector3.zero;
        }

        rb.AddForce(movement * speed);

        if (leftHand.Position.Y > spineMid.Position.Y) {
            movement = Vector3.forward;
        }
        else if (leftHand.Position.Y < spineMid.Position.Y) {
            movement = Vector3.back;
        }
        else {
            movement = Vector3.zero;
        }

        rb.AddForce(movement * speed);
    }

    void FixedUpdate()
    {
      
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 6)
        {
            wintText.text = "You win!";
        }
    }


    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

   

}
