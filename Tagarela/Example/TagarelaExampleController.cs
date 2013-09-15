using UnityEngine;
using System.Collections;

public class TagarelaExampleController : MonoBehaviour
{

    public void Start(){
    }

    public void OnGUI() {
        if (GUILayout.Button(" animation 1 ")) {
            GetComponent<Tagarela>().Play(0);
			//You also can Play using the animation name
			//GetComponent<Tagarela>().Play("bacon_0");
			//And use the Stop function, to stop the animation
			//GetComponent<Tagarela>().Stop(); 
        }

        if (GUILayout.Button(" animation 2 "))
        {
            GetComponent<Tagarela>().Play(1);
        }

        if (GUILayout.Button(" animation 3 "))
        {
            GetComponent<Tagarela>().Play(2);
        }
    }


}