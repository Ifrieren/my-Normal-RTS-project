using UnityEngine;
using RTS.EventSystem;
using UnityEngine.Rendering.Universal;
using System.ComponentModel.Design;
using System;
namespace RTS.Units
{
    public class BaseBuilding : BaseCommandable
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }

}
