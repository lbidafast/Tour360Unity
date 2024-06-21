using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public class MainVideoUIController : VideoUIController
    {
        public static MainVideoUIController Instance;

        protected override void Awake()
        {
            base.Awake();

            Instance = this;
        }
    }
}