using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Character: MonoBehaviour
    {
        private float _age;

        public float Age
        {
            get { return _age; }
            set
            {
                _age = value;
                SetAge(_age);
            }
        }

        public string Gender { get; set; }

        private void SetAge(float age)
        {
            this.transform.GetChild(1).GetComponent<Text>().text = age.ToString();
        }
    }
}
