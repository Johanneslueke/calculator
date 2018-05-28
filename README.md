# calculator
[![Build status](https://ci.appveyor.com/api/projects/status/nkrs4y3k4rxgnky9/branch/master?svg=true)](https://ci.appveyor.com/project/Johanneslueke/calculator/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/Johanneslueke/calculator/badge.svg?branch=master)](https://coveralls.io/github/Johanneslueke/calculator?branch=master)

----------------------------------------------

This project implements an Recursive Descent Parser and combines this parser with a simple GUI. The calculator functions are pretty much completely implemented but of course the implementation could needs some refinement.

The GUI and the parser are in two projects. The RDP_Arithmetic contains the parser and, you might have guessed it the  "Calculator eXtreme" contains the GUI and depends on the RDP_Arithmetic library.

I like this little project because the GUI is basically just one file figuring out what the user has pressed as button and then calling the library which does the actual work. The library is 100% reusable and can be developed independently from the GUI.
