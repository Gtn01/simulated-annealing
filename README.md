# Simulated annealing in Unity3D 
- **Unity 2019.4**

#

Simulated annealing is a powerful approach to find a local optimum for a complex problem. This algorithm is used to quickly get a good solution without searching through a large amount of potential solutions.

This implementation is an example on how to solve 3D  packing problem in a reasonable time. Automatic 3D packing is a very useful feature for SLS 3D printing technology.

This project is developed to work with any type of [stl file](https://github.com/QuantumConcepts/STLdotNET), but you are free to use any type of data structure consisting of a set of vertices and triangles.

# Simulated annealing in 7 steps


1. First, an [Octree structure](https://fr.wikipedia.org/wiki/Octree) is computed for each mesh in each desired rotation. Based on vertices density, it provides a useful data structure for annealing interations

2. The system is initialized in a pseudo-random configuration, depending on the type of minimization required (volume or height)

3. Compute the **energy** of the current system

4. **Move** and/or **rotate** one or more meshes in the current state

5. **Check all collision** between meshes using octree structure, the solution is rejected if a collision is detected

6. **Compute and compare the energy** with the last iteration. If a better solution is found, the solution is considered, ortherwise it's rejected.

7. **Repeat annealing** iteration until the solution is acceptable. 

# How to get better solutions

You will notice that the slower the energy is decreased, better the solution is (in this specific case).

Reducing the range of motion for meshes in each annealing iteration will help you to explore a larger amount of solution.

Otherwise, the system will converge in a very local solution.


# Benchy boat example : Height minimization

Tested with 164 benchy boat
![drawing](https://i.imgur.com/NN9nz2Y.png)

Especially useful to prepare a print with a bunch of meshes **regardless of their size, geometry, or complexity**

# Benchy boat example : Volume minimization

Tested with 164 benchy boats
![drawing](https://i.imgur.com/CLno3bJ.png)

Especially useful to enclose a large amount of small pieces

# Quick start

## Run sample scene width your own 3D models

**scene : simulated_annealing** import your model and click run simulared annealing alorithm

![drawing](https://i.imgur.com/8zV0U4Y.png)


## Instantiate a new Packing3D

    packing3D.MainPack3D p3d = new packing3D.MainPack3D();


## Configure model
    p3d._printerWidth 
    p3d._printerHeight
    p3d._printerDepth 
    p3d._printerCorner
    p3d.bvhDetail
    p3d.annealingIterations
    p3d.packingIteration
    p3d.minimalOffset
    p3d.meshes
    p3d.cpu

## Configure listenners

    p3d.onNewShapeAddedIntoModel( string name, bool success )
    p3d.onPackingBegin( int packingIndex, bool success )
    p3d.onAnnealProgress( Model model, int percentage, float bestEnergy, float currentEnergy )
    p3d.onAnnealDone()
    p3d.onNewSolutionFound(Model model, float newEnergy )
    p3d.onPackingDone()


## Run 
        
    Thread thread = new Thread(p3d.runMultiThread);
    thread.Start();  




## Results
![drawing](https://i.imgur.com/EaFw6rJ.png)
![drawing](https://i.imgur.com/pMvKFE5.png)


