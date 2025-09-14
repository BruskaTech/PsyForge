//Copyright (c) 2025 Bruska Technologies LLC (James Bruska)

//This file is part of OpenField.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using PsyForge.Experiment;

public class TestConstants : ExperimentConstants {
    public virtual int keycodeDisplayDurationMs => 1500;
    public virtual int numTrialsPerSession => 2;
    public virtual bool debug => false;
}