using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Globalization;
using Leap.Unity.Attributes;
using TMPro;
//standard imports

/*
 * signTemplate acts as a container of information relevant to individual signs. 
 * Namely: 
 * -the vectors/3D graph points for the reference version of the hand pose
 * -the filename which the hand pose was pulled from
 * -the simplified name of the pose (the filename and reference tag cut off)
 * -the acceptable difference for this particular pose
 */
public struct signTemplate
{
    public double[,] data;
    public string filename;
    public string sign;
    public double precision;
}

public class SpaceBar : MonoBehaviour
{
    Controller cont;            //controller from the device
    signTemplate baseHand;      //base template for use in comparison and loops
    public TextMeshPro textmeshPro;     //base mesh for output of current sign
    public GameObject bed;
    /*to control the text printout and prevent the output flickering all over the place, time delay on printout*/

    /*text to insert to the default text given to user for a sign to try*/
    string signToTry = "A";
    
    string currentSign = "";    //sign that the current frame returns as a string
    string previousSign = "";   //the last sign as a string
    string signSoFar = "";         //string of the signs concatenated. Hypothetically for making strings/sentences with sign

    private double thumbWeight = 1.5;   //increased weight to apply to the thumb, as it is highly important to many signs
    public double defaultPrecision = 10.01;   //default precision for a hand pose.
    public double maxDistance = 35.0;

    public bool ChallengeActive = false;
    public bool ChallengePause = false;
    public long currentTick = 0;
    public long nextTick = 0;
    public long lastSign = 0;
    

    List<signTemplate> baseHands;       //list of possible signs currently defined

    // Use this for initialization
    void Start()
    {
        baseHands = new List<signTemplate>();
        cont = new Controller();

        cont.FrameReady += handleFrameReady;        //add listener that notifies if there's a valid tracking frame
        //bed.SetActive(false);
        /*generate the signing dictionary*/
        //baseHand = readTheStuffs("OpenPalmReference.txt");
        //baseHands.Add(baseHand);
        baseHand = readTheStuffs("aReference.txt");
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("bReference.txt");
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("cReference.txt");
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("dReference.txt");
        baseHand.precision = 11.5;
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("eReference.txt");
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("fReference.txt");
        baseHand.precision = 13;
        baseHands.Add(baseHand);
        baseHand = readTheStuffs("gReference.txt");
        baseHands.Add(baseHand);
        //baseHand = readTheStuffs("hReference.txt");
        //baseHands.Add(baseHand);
        //baseHand = readTheStuffs("iReference.txt");
        //baseHands.Add(baseHand);
        //baseHand = readTheStuffs("tReference.txt");
        //baseHands.Add(baseHand);
        //baseHand = readTheStuffs("mReference.txt");
        //baseHands.Add(baseHand);
        //baseHand = readTheStuffs("nReference.txt");
        //baseHands.Add(baseHand);
    }

    //handler for FrameReady event, when there's a valid tracking frame.
    void handleFrameReady(object sender, EventArgs e)
    {
        try
        { 
            Frame framey = cont.Frame();        //get current frame
            int i = 0;
            Hand compareHand;
            currentTick = DateTime.Now.Ticks;
            if (nextTick <= currentTick && framey.CurrentFramesPerSecond > 0 && framey.Hands.Count > 0)        //if there are multiple tracking frames and hand in frame
            {
                nextTick = DateTime.Now.Ticks + 1500000;   //10 million ticks = 1 second
                compareHand = framey.Hands[0];
                
                double compareVal = 100;    //default value above any leap motion can give
                double newCompareVal = 0;

                foreach (signTemplate baseHand in baseHands)        //for each reference hand, compare with current hand. find minimum comprison value and sign associated
                {
                    newCompareVal = compareHands(baseHand.data, compareHand);
                    i++;
                    if (newCompareVal < compareVal)
                    {
                        compareVal = newCompareVal;
                        currentSign = baseHand.sign;
                    }
                }
                //print(currentSign + " " + compareVal);


                //int score = (100 - (int)(100 * (compareVal - baseHands[i - 1].precision) / (maxDistance - baseHands[i - 1].precision)));
                //if(score>100)
                //{
                    //score = 100;
                //}
                if (i != 0 && compareVal <= baseHands[i - 1].precision && !ChallengeActive) //if found a valid sign, compare the outcome with the precision allowed by that referece sign
                {
                    
                    //UpdateText("You signed: "+signSoFar
                    //UpdateText("You signed: \n" + currentSign.ToUpper()+"\n Score:  "+score+"%"); //update the output with the sign found
                    UpdateText("You signed: \n" + currentSign.ToUpper() + "\n");

                    /* handling for sentences */
                    /*if (previousSign != currentSign)    //if the sign has changed,
                    {
                        if (currentSign == "openpalm")      //used as space character. depricated by the updated handling of reference files
                        {
                            signSoFar += " ";
                        }
                        else
                        {
                            signSoFar += currentSign;
                            
                        }

                        previousSign = currentSign;
                    }*/
                    lastSign = DateTime.Now.Ticks;
                    
                }
                else if(ChallengeActive)
                {
                    //Challenge code
                    if (previousSign != currentSign && ChallengeActive)    //if the sign has changed,
                    {
                        if (currentSign == "b" && signSoFar == "")      //used as space character. depricated by the updated handling of reference files
                        {
                            signSoFar += "b";
                        }
                        else if (currentSign == "e" && signSoFar == "b")      //used as space character. depricated by the updated handling of reference files
                        {
                            signSoFar += "e";
                        }
                        else if (currentSign == "d" && signSoFar == "be")      //used as space character. depricated by the updated handling of reference files
                        {
                            signSoFar += "d";
                        }
                    }

                    UpdateText("Progress: \n" + signSoFar.ToUpper() + "\n");
                }
                else if(currentTick - lastSign >= 15000000 )
                {
                    //UpdateText("You signed: \n" + currentSign.ToUpper() + "\n");
                    //UpdateTextBad("You signed: \n" + currentSign.ToUpper() + "\n Score:  " + score + "%"); //update the output with the sign found

                    UpdateText("\nTry: "+signToTry+"\n");
                }
                
                //print(signSoFar);
            }
        }
        catch(Exception except)
        {
            ;   //shouldn't ever hit, but maintained for debugging purposes. Originally needed before good management of the event listener.
        }
        

        return;
    }

    //update the text based on the standard timer
    void UpdateText(string output)
    {
        //if (Time.time > nextActionTime)
        //{
            //nextActionTime = Time.time + period;            
            //textmeshPro = GetComponent<TextMeshPro>();
            textmeshPro.color = new Color32(0, 255, 0, 255);
            //textmeshPro.SetText("You signed it!");
            textmeshPro.text = (output);
            //print(textmeshPro.text);
            textmeshPro.ForceMeshUpdate();
            //textChanger sn = GetComponent<textChanger>();
            //sn.changeText("You correctly signed it!");

        //}


        return;
    }

    //update the text based on a timer for transitioning back to default text
    void UpdateTextBad(string output)
    {
        //if (Time.time > nextResetTime)
        //{
            //nextActionTime = Time.time + resetPeriod;


            //textmeshPro = GetComponent<TextMeshPro>();
            textmeshPro.color = new Color32(255, 0, 0, 255);
            //textmeshPro.SetText("You signed it!");
            textmeshPro.text = (output);
            //print(textmeshPro.text);
            textmeshPro.ForceMeshUpdate();
            //textChanger sn = GetComponent<textChanger>();
            //sn.changeText("You correctly signed it!");

        //}


        return;
    }


    void Update()
    {
        bool createRefs = false;
        //used for building dictionary. not sure why it only will work once per run of program.
        //has an exception around file sharing/permissions, but writes and reads the files anyway.
        if (createRefs && Input.anyKeyDown)
        {
            string keyPressed = Input.inputString;
            Frame framey = cont.Frame();
            if (framey.CurrentFramesPerSecond > 0 && framey.Hands.Count > 0)    //if in a valid frame
            {
                Hand whyDoesThisNotWork = framey.Hands[0];  //but it does work now! 
                string filename = saveTheStuffs(whyDoesThisNotWork, (keyPressed + "Reference.txt"));    //save file based on key pressed.

                signTemplate testymctestface = readTheStuffs(filename); //read back the file
                print(compareHands(testymctestface.data, whyDoesThisNotWork));

                if (compareHands(testymctestface.data, whyDoesThisNotWork) > 0.01)
                {
                    throw new InvalidOperationException("Hand written and hand read don't match");
                }
            }
            else
            {
                print("no hands");
            }
        }


        if (!ChallengeActive && Input.GetButtonDown("StartChallenge"))
        {
            UpdateText("Challenge:\n Spell BED");
            ChallengeActive = true;
            nextTick = DateTime.Now.Ticks + 20000000;
            signSoFar = "";
        }
        
        if (ChallengeActive)
        {
            if (signSoFar.Contains("bed"))
            {
                UpdateText("Challenge Complete");
                bed.SetActive(true);
                
                //ChallengeActive = false;
            }
        }

        if(ChallengeActive && Input.GetButtonDown("ResetChallenge"))
        {
            bed.SetActive(false);
            ChallengeActive = false;
            signSoFar = "";
        }
        return;
    }

    
    private void OnDestroy()
    {
        cont.FrameReady -= handleFrameReady; //remove listener
    }

    public double compareHands(double[,] refHandMatrix, Hand currentHand)
    {

        //massive hardcoded assignments. The loops got screwy. get 4 bones for each finger, and the palm normal vector.
        double[,] currHandMatrix = new double[21, 3];

        double[] thumb1 = new double[3] { currentHand.Fingers[0].bones[0].Direction.x * thumbWeight, currentHand.Fingers[0].bones[0].Direction.y * thumbWeight, currentHand.Fingers[0].bones[0].Direction.z * thumbWeight };
        double[] thumb2 = new double[3] { currentHand.Fingers[0].bones[1].Direction.x * thumbWeight, currentHand.Fingers[0].bones[1].Direction.y * thumbWeight, currentHand.Fingers[0].bones[1].Direction.z * thumbWeight };
        double[] thumb3 = new double[3] { currentHand.Fingers[0].bones[2].Direction.x * thumbWeight, currentHand.Fingers[0].bones[2].Direction.y * thumbWeight, currentHand.Fingers[0].bones[2].Direction.z * thumbWeight };
        double[] thumb4 = new double[3] { currentHand.Fingers[0].bones[3].Direction.x * thumbWeight, currentHand.Fingers[0].bones[3].Direction.y * thumbWeight, currentHand.Fingers[0].bones[3].Direction.z * thumbWeight };

        double[] index1 = new double[3] { currentHand.Fingers[1].bones[0].Direction.x, currentHand.Fingers[1].bones[0].Direction.y, currentHand.Fingers[1].bones[0].Direction.z };
        double[] index2 = new double[3] { currentHand.Fingers[1].bones[1].Direction.x, currentHand.Fingers[1].bones[1].Direction.y, currentHand.Fingers[1].bones[1].Direction.z };
        double[] index3 = new double[3] { currentHand.Fingers[1].bones[2].Direction.x, currentHand.Fingers[1].bones[2].Direction.y, currentHand.Fingers[1].bones[2].Direction.z };
        double[] index4 = new double[3] { currentHand.Fingers[1].bones[3].Direction.x, currentHand.Fingers[1].bones[3].Direction.y, currentHand.Fingers[1].bones[3].Direction.z };

        double[] middle1 = new double[3] { currentHand.Fingers[2].bones[0].Direction.x, currentHand.Fingers[2].bones[0].Direction.y, currentHand.Fingers[2].bones[0].Direction.z };
        double[] middle2 = new double[3] { currentHand.Fingers[2].bones[1].Direction.x, currentHand.Fingers[2].bones[1].Direction.y, currentHand.Fingers[2].bones[1].Direction.z };
        double[] middle3 = new double[3] { currentHand.Fingers[2].bones[2].Direction.x, currentHand.Fingers[2].bones[2].Direction.y, currentHand.Fingers[2].bones[2].Direction.z };
        double[] middle4 = new double[3] { currentHand.Fingers[2].bones[3].Direction.x, currentHand.Fingers[2].bones[3].Direction.y, currentHand.Fingers[2].bones[3].Direction.z };

        double[] ring1 = new double[3] { currentHand.Fingers[3].bones[0].Direction.x, currentHand.Fingers[3].bones[0].Direction.y, currentHand.Fingers[3].bones[0].Direction.z };
        double[] ring2 = new double[3] { currentHand.Fingers[3].bones[1].Direction.x, currentHand.Fingers[3].bones[1].Direction.y, currentHand.Fingers[3].bones[1].Direction.z };
        double[] ring3 = new double[3] { currentHand.Fingers[3].bones[2].Direction.x, currentHand.Fingers[3].bones[2].Direction.y, currentHand.Fingers[3].bones[2].Direction.z };
        double[] ring4 = new double[3] { currentHand.Fingers[3].bones[3].Direction.x, currentHand.Fingers[3].bones[3].Direction.y, currentHand.Fingers[3].bones[3].Direction.z };

        double[] pinky1 = new double[3] { currentHand.Fingers[4].bones[0].Direction.x, currentHand.Fingers[4].bones[0].Direction.y, currentHand.Fingers[4].bones[0].Direction.z };
        double[] pinky2 = new double[3] { currentHand.Fingers[4].bones[1].Direction.x, currentHand.Fingers[4].bones[1].Direction.y, currentHand.Fingers[4].bones[1].Direction.z };
        double[] pinky3 = new double[3] { currentHand.Fingers[4].bones[2].Direction.x, currentHand.Fingers[4].bones[2].Direction.y, currentHand.Fingers[4].bones[2].Direction.z };
        double[] pinky4 = new double[3] { currentHand.Fingers[4].bones[3].Direction.x, currentHand.Fingers[4].bones[3].Direction.y, currentHand.Fingers[4].bones[3].Direction.z };

        double[] palm = new double[3] { currentHand.PalmNormal.x, currentHand.PalmNormal.y, currentHand.PalmNormal.z };

        //write the various bones to a singe matrix.
        for (int i = 0; i < 3; i++)
        {
            currHandMatrix[0, i] = palm[i];
            currHandMatrix[1, i] = thumb1[i];
            currHandMatrix[2, i] = thumb2[i];
            currHandMatrix[3, i] = thumb3[i];
            currHandMatrix[4, i] = thumb4[i];
            currHandMatrix[5, i] = index1[i];
            currHandMatrix[6, i] = index2[i];
            currHandMatrix[7, i] = index3[i];
            currHandMatrix[8, i] = index4[i];
            currHandMatrix[9, i] = middle1[i];
            currHandMatrix[10, i] = middle2[i];
            currHandMatrix[11, i] = middle3[i];
            currHandMatrix[12, i] = middle4[i];
            currHandMatrix[13, i] = ring1[i];
            currHandMatrix[14, i] = ring2[i];
            currHandMatrix[15, i] = ring3[i];
            currHandMatrix[16, i] = ring4[i];
            currHandMatrix[17, i] = pinky1[i];
            currHandMatrix[18, i] = pinky2[i];
            currHandMatrix[19, i] = pinky3[i];
            currHandMatrix[20, i] = pinky4[i];
        }

        //generate the distance/diff of the hands
        double result = 0;
        double diff = 0;
        for (int j = 0; j < 16; j++)
        {
            for (int k = 0; k < 3; k++)
            {
                if ((Math.Abs(refHandMatrix[j, k] - currHandMatrix[j, k])) >= .001)
                {
                    diff = Math.Abs(refHandMatrix[j, k] - currHandMatrix[j, k]);
                    result += diff;
                }
                //print("result diff: " + refHandMatrix[j, k] + " " + currHandMatrix[j, k] + " = " + diff);
            }
        }
        return result;
    }

    //write hand to file.
    //The other one
    private string saveTheStuffs(Hand currentHand, string filename)
    {
        double[] thumb1 = new double[3] { currentHand.Fingers[0].bones[0].Direction.x * thumbWeight, currentHand.Fingers[0].bones[0].Direction.y * thumbWeight, currentHand.Fingers[0].bones[0].Direction.z * thumbWeight };
        double[] thumb2 = new double[3] { currentHand.Fingers[0].bones[1].Direction.x * thumbWeight, currentHand.Fingers[0].bones[1].Direction.y * thumbWeight, currentHand.Fingers[0].bones[1].Direction.z * thumbWeight };
        double[] thumb3 = new double[3] { currentHand.Fingers[0].bones[2].Direction.x * thumbWeight, currentHand.Fingers[0].bones[2].Direction.y * thumbWeight, currentHand.Fingers[0].bones[2].Direction.z * thumbWeight };
        double[] thumb4 = new double[3] { currentHand.Fingers[0].bones[3].Direction.x * thumbWeight, currentHand.Fingers[0].bones[3].Direction.y * thumbWeight, currentHand.Fingers[0].bones[3].Direction.z * thumbWeight };

        double[] index1 = new double[3] { currentHand.Fingers[1].bones[0].Direction.x, currentHand.Fingers[1].bones[0].Direction.y, currentHand.Fingers[1].bones[0].Direction.z };
        double[] index2 = new double[3] { currentHand.Fingers[1].bones[1].Direction.x, currentHand.Fingers[1].bones[1].Direction.y, currentHand.Fingers[1].bones[1].Direction.z };
        double[] index3 = new double[3] { currentHand.Fingers[1].bones[2].Direction.x, currentHand.Fingers[1].bones[2].Direction.y, currentHand.Fingers[1].bones[2].Direction.z };
        double[] index4 = new double[3] { currentHand.Fingers[1].bones[3].Direction.x, currentHand.Fingers[1].bones[3].Direction.y, currentHand.Fingers[1].bones[3].Direction.z };

        double[] middle1 = new double[3] { currentHand.Fingers[2].bones[0].Direction.x, currentHand.Fingers[2].bones[0].Direction.y, currentHand.Fingers[2].bones[0].Direction.z };
        double[] middle2 = new double[3] { currentHand.Fingers[2].bones[1].Direction.x, currentHand.Fingers[2].bones[1].Direction.y, currentHand.Fingers[2].bones[1].Direction.z };
        double[] middle3 = new double[3] { currentHand.Fingers[2].bones[2].Direction.x, currentHand.Fingers[2].bones[2].Direction.y, currentHand.Fingers[2].bones[2].Direction.z };
        double[] middle4 = new double[3] { currentHand.Fingers[2].bones[3].Direction.x, currentHand.Fingers[2].bones[3].Direction.y, currentHand.Fingers[2].bones[3].Direction.z };

        double[] ring1 = new double[3] { currentHand.Fingers[3].bones[0].Direction.x, currentHand.Fingers[3].bones[0].Direction.y, currentHand.Fingers[3].bones[0].Direction.z };
        double[] ring2 = new double[3] { currentHand.Fingers[3].bones[1].Direction.x, currentHand.Fingers[3].bones[1].Direction.y, currentHand.Fingers[3].bones[1].Direction.z };
        double[] ring3 = new double[3] { currentHand.Fingers[3].bones[2].Direction.x, currentHand.Fingers[3].bones[2].Direction.y, currentHand.Fingers[3].bones[2].Direction.z };
        double[] ring4 = new double[3] { currentHand.Fingers[3].bones[3].Direction.x, currentHand.Fingers[3].bones[3].Direction.y, currentHand.Fingers[3].bones[3].Direction.z };

        double[] pinky1 = new double[3] { currentHand.Fingers[4].bones[0].Direction.x, currentHand.Fingers[4].bones[0].Direction.y, currentHand.Fingers[4].bones[0].Direction.z };
        double[] pinky2 = new double[3] { currentHand.Fingers[4].bones[1].Direction.x, currentHand.Fingers[4].bones[1].Direction.y, currentHand.Fingers[4].bones[1].Direction.z };
        double[] pinky3 = new double[3] { currentHand.Fingers[4].bones[2].Direction.x, currentHand.Fingers[4].bones[2].Direction.y, currentHand.Fingers[4].bones[2].Direction.z };
        double[] pinky4 = new double[3] { currentHand.Fingers[4].bones[3].Direction.x, currentHand.Fingers[4].bones[3].Direction.y, currentHand.Fingers[4].bones[3].Direction.z };

        double[] palm = new double[3] { currentHand.PalmNormal.x, currentHand.PalmNormal.y, currentHand.PalmNormal.z };

        System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
        file.WriteLine(palm[0] + " " + palm[1] + " " + palm[2]);
        file.WriteLine(thumb1[0] + " " + thumb1[1] + " " + thumb1[2]);
        file.WriteLine(thumb2[0] + " " + thumb2[1] + " " + thumb2[2]);
        file.WriteLine(thumb3[0] + " " + thumb3[1] + " " + thumb3[2]);
        file.WriteLine(thumb4[0] + " " + thumb4[1] + " " + thumb4[2]);
        file.WriteLine(index1[0] + " " + index1[1] + " " + index1[2]);
        file.WriteLine(index2[0] + " " + index2[1] + " " + index2[2]);
        file.WriteLine(index3[0] + " " + index3[1] + " " + index3[2]);
        file.WriteLine(index4[0] + " " + index4[1] + " " + index4[2]);
        file.WriteLine(middle1[0] + " " + middle1[1] + " " + middle1[2]);
        file.WriteLine(middle2[0] + " " + middle2[1] + " " + middle2[2]);
        file.WriteLine(middle3[0] + " " + middle3[1] + " " + middle3[2]);
        file.WriteLine(middle4[0] + " " + middle4[1] + " " + middle4[2]);
        file.WriteLine(ring1[0] + " " + ring1[1] + " " + ring1[2]);
        file.WriteLine(ring2[0] + " " + ring2[1] + " " + ring2[2]);
        file.WriteLine(ring3[0] + " " + ring3[1] + " " + ring3[2]);
        file.WriteLine(ring4[0] + " " + ring4[1] + " " + ring4[2]);
        file.WriteLine(pinky1[0] + " " + pinky1[1] + " " + pinky1[2]);
        file.WriteLine(pinky2[0] + " " + pinky2[1] + " " + pinky2[2]);
        file.WriteLine(pinky3[0] + " " + pinky3[1] + " " + pinky3[2]);
        file.WriteLine(pinky4[0] + " " + pinky4[1] + " " + pinky4[2]);

        file.Close();

        return filename;
    }

    //read from file
    private signTemplate readTheStuffs(string fileName)
    {
        double[,] handArray = new double[21, 3];

        System.IO.StreamReader file = new System.IO.StreamReader(Application.dataPath+"/templates/"+fileName);
        String line;
        int i = 0;
        while ((line = file.ReadLine()) != null)
        {
            var tmp = line.Split(new[] { ' ' });
            handArray[i, 0] = Double.Parse(tmp[0], CultureInfo.InvariantCulture);
            handArray[i, 1] = Double.Parse(tmp[1], CultureInfo.InvariantCulture);
            handArray[i, 2] = Double.Parse(tmp[2], CultureInfo.InvariantCulture);
            ++i;
        }

        //build template hand struct.
        signTemplate refHand;
        refHand.data = handArray;
        refHand.filename = fileName;
        refHand.sign = fileName.Replace("Reference.txt", "");
        refHand.precision = defaultPrecision;
        return refHand;
    }
}
