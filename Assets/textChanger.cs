using UnityEngine;
using TMPro;
using System.Collections;



public class textChanger : MonoBehaviour {

    public TextMeshPro textmeshPro;

    public void changeText(string s)
    {
        print(s);
        textmeshPro = GetComponent<TextMeshPro>();
        textmeshPro.color = new Color32(0, 255, 0, 255);
        textmeshPro.SetText(s);
        textmeshPro.text = (s);
        textmeshPro.ForceMeshUpdate();

        print(textmeshPro.text);
    }
}
