# Startup Scene

This document describes how the startup scene works and how to create your own.

## You don't always need to make your own (read this first)

Before you decide to make your own startup scene, if you only want to add features to the startup scene there is probably a better option.
Just add it to your experiment scene and run them in the InitialStates section.


## How to Make your own startup scene

Things you have to do (in order):

1. Call `await Config.SetupExperimentConfig(<configName>);`
    - this sets the experiment config
    - this is separate so that you can test loading the config before starting the exeriment
2. Call `LaunchExperiment(<subject>, <sessionNumber>);`
    - this launches the experiment.
