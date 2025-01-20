using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required when Using UI elements.
using TMPro;

public class spawner : MonoBehaviour 
{
    [SerializeField]
    private int nu;
	public TMP_InputField mainInputField;

	// Checks if there is anything entered into the input field.
	void LockInput(TMP_InputField input)
	{
		if (input.text.Length > 0) 
		{
            PlayerPrefs.SetString("B" + nu.ToString() , input.text);
			Debug.Log("Text has been entered");
            input.text = PlayerPrefs.GetString("B" + nu.ToString());
		}
		else if (input.text.Length == 0) 
		{
			Debug.Log("Main Input Empty");
		}
	}

	public void Start()
	{
        mainInputField = gameObject.GetComponent<TMP_InputField>();
		//Use onSubmit
		mainInputField.onSubmit.AddListener(delegate{LockInput(mainInputField);});
	}
}

//출처: https://naakjii.tistory.com/83 [NJSUNG BLOG:티스토리]