//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using PsyForge;
using PsyForge.Extensions;
using PsyForge.Utilities;

namespace PsyForgeTests {

    public class MainManagerTests {
        // -------------------------------------
        // Globals
        // -------------------------------------

        bool isSetup = false;

        const double ONE_FRAME_MS = 1000.0 / 120.0;

        // -------------------------------------
        // Setup
        // -------------------------------------

        [UnitySetUp]
        public IEnumerator Setup() {
            if (!isSetup) {
                isSetup = true;
                SceneManager.LoadScene("manager");
                yield return null; // Wait for MainManager Awake call
            }
        }

        // -------------------------------------
        // General Tests
        // -------------------------------------

        [Test]
        public void Creation() {
            Assert.AreNotEqual(null, MainManager.Instance);
        }

        // Async Delay has 9ms leniency (because it's bad)
        [UnityTest]
        public IEnumerator Delay() {
            var start = Clock.UtcNow;
            yield return Timing.Delay(1000);
            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + ONE_FRAME_MS);
        }

        
    }

}


