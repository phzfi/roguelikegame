using UnityEngine;
using System.Collections;

// All code and comments in English
// Class names start with capital letter
// All curly brackets are placed on their own line
public class ExampleScript : MonoBehaviour
{                                              
	// First all public member variables											
	public float m_publicMemberVariable;        // Variable names start with lower case
	public AnimationCurve m_anotherPublic;      // Member variables start with m_

	// Then all private member variables
	private Vector3 m_privateVector;            // All variables and function names use camelCase
	private float m_secondVariable;             // e.g. use testVariable instead of test_variable
	private static float sm_someStatic = 0.1f;  // Prefix static member variables with sm_

	public float PrivateVectorX					// Property names start with capital letter
	{
		get { return m_privateVector.x; }		// With properties it's ok to place braces to the same line
	}

	// Then all functions, preferably starting with Awake(), Start() and Update()
	void Start()
	{
		m_privateVector = transform.localPosition;
	}
	
	// Please don't write as many comments as in this file :)
	void Update()
	{
		float someLocalVariable = 1.0f;

		// No spaces within or before parentheses
		someLocalVariable = SomePrivateFunctionThatDoesSomeThing(someLocalVariable);
		someLocalVariable *= sm_someStatic;

        transform.localPosition = m_privateVector;
	}

	// All function names start with capital letter
	// Try to use as descriptive names as possible, even if they are longer
	private float SomePrivateFunctionThatDoesSomeThing(float someFloatParameter)
	{
		return someFloatParameter * 2.0f;
	}
}
