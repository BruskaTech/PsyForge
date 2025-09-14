//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using PsyForge.Experiment;
using PsyForge.Localization;
using System.Collections.Generic;
using Codice.CM.Common;

public class TestExperiment : ExperimentBase<TestExperiment, TestSession, TestTrial, TestConstants> {
    protected override void AwakeOverride() { }

    protected override async Awaitable InitialStates() { await Task.CompletedTask; }
    protected override async Awaitable FinalStates() { await Task.CompletedTask; }
    protected override Awaitable PracticeTrialStates(CancellationToken ct) {
        throw new EndSessionException(); // This exception will end either the practice trials or the normal trials, depending on which is currently running.
    }

    protected override async Awaitable TrialStates(CancellationToken ct) {
        // Show a starting message and wait for a key press to begin.
        // This automatically puts a "Press Any Key to Continue" message at the bottom.
        await ExpHelpers.PressAnyKey("session start", LangStrings.SessionStart(), ct);

        // Tell the user to press one of two keys, and wait for them to do so, and then log it.
        textDisplayer.Display("Press 1 or 2", text: LangStrings.Press1or2());
        var keyOptions = new List<KeyCode>() { KeyCode.Alpha1, KeyCode.Alpha2 };
        var selectedKey = await inputManager.WaitForKey(keyOptions, ct: ct);
        eventReporter.LogTS("key selection", new() {
            { "keyOptions", keyOptions },
            { "selectedKey", selectedKey },
        });
        textDisplayer.Clear();

        // Delay for a few seconds so the user can see what they pressed.
        textDisplayer.Display("You pressed: " + selectedKey, text: LangStrings.YouPressed(selectedKey));
        await Task.Delay(CONSTANTS.keycodeDisplayDurationMs, ct);

        // End the session if we have completed enough trials.
        if (session.TrialNum >= CONSTANTS.numTrialsPerSession) { // Trial number is 1-indexed, so this is after numTrialsPerSession (2) trials.
            EndCurrentSession(); // This also will end the current set of trials (practice or normal).
        }
    }

    

    

}