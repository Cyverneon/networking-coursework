using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetAnimatorBool : StateMachineBehaviour 
{
    [Tooltip("Name of the boolean to set")]
    [SerializeField] private string _name;

    [Tooltip("whether to set the parameter to true or false")]
    [SerializeField] private bool _enable = true;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetBool(_name, _enable);
    }

}
