# ESAPI Breast Skin Flash Tool

## Description
This script automates the "Pseudo Skin Flash" strategy for VMAT Breast planning in Varian Eclipse. It ensures surface dosimetric coverage and robustness against respiratory motion.

## Key Features
* **Virtual Bolus Generation:** Automates the creation of optimization structures in the air.
* **Body Swap Algorithm:** Implements a logic to swap the `EXTERNAL` and `CONTROL` structures to extend the calculation volume without corrupting the DICOM data.
* **Auto-Density:** Automatically assigns customizable Hounsfield Units (HU) to the flash region.
* **OAR Protection:** Boolean logic to trim expansions from lungs and heart.

## Technologies
* C# (.NET Framework)
* Varian ESAPI (Eclipse Scripting API)
* WPF (Windows Presentation Foundation) for the User Interface.

## Disclaimer
This tool is for research and educational purposes. Use under clinical supervision.
