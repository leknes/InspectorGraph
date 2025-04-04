﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InspectorGraph
{
    public static class UnityLogger
    {
        public static void InvokeWithLogDisabled(Action action)
        {
            Debug.unityLogger.logEnabled = false;

            action();

            Debug.unityLogger.logEnabled = true;
        }
    }
}
