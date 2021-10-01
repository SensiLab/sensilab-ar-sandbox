//  
//  UI_HoldButton.cs
//
//	Copyright 2021 SensiLab, Monash University <sensilab@monash.edu>
//
//  This file is part of sensilab-ar-sandbox.
//
//  sensilab-ar-sandbox is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  sensilab-ar-sandbox is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with sensilab-ar-sandbox.  If not, see <https://www.gnu.org/licenses/>.
//

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace ARSandbox
{
    public class UI_HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent OnHoldFunction;
        public float DispatchFrequency = 1 / 60f;
        public float DispatchInitialWait = 1 / 4f;

        private bool waitingOnRelease = false;
        private Coroutine holdFunctionCoroutine;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!waitingOnRelease)
            {
                holdFunctionCoroutine = StartCoroutine(RunHoldFunction());
                waitingOnRelease = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopCoroutine(holdFunctionCoroutine);
            waitingOnRelease = false;
        }

        private IEnumerator RunHoldFunction()
        {
            OnHoldFunction.Invoke();
            yield return new WaitForSeconds(DispatchInitialWait + DispatchFrequency);
            while (true)
            {
                OnHoldFunction.Invoke();
                yield return new WaitForSeconds(DispatchFrequency);
            }
        }
    }
}
