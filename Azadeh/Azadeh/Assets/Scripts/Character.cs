using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Character : MonoBehaviour
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

        private bool _hasGlasses;

        public bool HasGlasses
        {
            get { return _hasGlasses; }
            set
            {
                _hasGlasses = value;
                SetGlasses(_hasGlasses);
            }
        }

        private string _gender;

        public string Gender
        {
            get { return _gender; }
            set
            {
                _gender = value;
                SetGender(_gender);
            }
        }

        private string _emotion;

        public string Emotion
        {
            get { return _emotion; }
            set
            {
                _emotion = value;
                SetEmotion(this.transform.gameObject, _emotion);
            }
        }



        public float EmotionValue { get; set; }


        private void SetAge(float age)
        {
            // var glasses = FindChild(this.transform.gameObject, "Age").
            this.transform.GetChild(0).GetComponent<Text>().text = age.ToString();
        }

        private void SetGlasses(bool hasglasses)
        {
            var glasses = FindChild(this.transform.gameObject, "glasses");
            if (!hasglasses)
            {

                glasses.gameObject.SetActive(false);

            }
        }

        private void SetGender(string gender)
        {
            var bowpink = FindChild(this.transform.gameObject, "bow-pink");
            if (gender != "female")
            {
                bowpink.gameObject.SetActive(false);

            }
        }

        private void SetEmotion(GameObject gObject, string emotion)
        {
            for (int i = 0; i < gObject.transform.childCount; i++)
            {
                var tmp = gObject.transform.GetChild(i);
                if (gObject.transform.GetChild(i).name.StartsWith("mouth-") && gObject.transform.GetChild(i).name != string.Format("mouth-{0}", emotion))
                {
                    tmp.gameObject.SetActive(false);
                }
            }
        }

        private Transform FindChild(GameObject gObject, string gObjectName)
        {
            for (int i = 0; i < gObject.transform.childCount; i++)
            {
                var tmp = gObject.transform.GetChild(i);
                if (gObject.transform.GetChild(i).name == gObjectName)
                {
                    return tmp;
                }
            }
            return null;

        }


    }
}
