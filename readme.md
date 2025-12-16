

## Worksheet 2



## Literature Review 

The boids model was introduced by Craig Reynolds in 1987 \cite{Craig1987Boids} and is a highly utilized model in computer graphics. Reynolds' goal was to create an alternate method to manually setting paths of birds, using a particle system approach, where the boids were treated as particles. Therefore, each boid was simulated individually using the three rules of seperation, alignment and cohesion.

Schools of fish have been examined to figure out how they are formed \cite{Brian1982Fish}. Fish use their vision to maintain a position and angle in the school. They also observed that the fish do not follow a strict pattern, with a "preferred" distance and angle from its nearest neighbor, but they are not maintained rigidly. These findings show that fish have similar behavior patterns to birds, which has lead to the development of fish simulations with boids \cite{Kawabayashi2008boids}.

## Profiling Results

![CPUTime](Images/performanceBefore.png)


In the current boids implementation, there is severe performance limitations and issues which will need to be addressed in the next iteration. 

The main bottleneck is the movement algorithm that each boid is running every frame, which is contributing to over 75% of the usage of the main thread, as shown below.

![CPUTime](Images/percentageBefore.png)

All the computing for the movement algorithm is done on the CPU, this causes significant strain on the CPU, being way above the target frame rate of 60fps, as shown below. The GPU is not being used at all, other than by default through Unity, so there is no strain on the GPU.

![CPUTime](Images/CPUtimeBefore.png)



The algorithm checks against every other boid, therefore every boid checks every other boid active in the scene, this creates a time complexity of O(n²), which severely limits the amount of boids that can be active in a scene at once. It was found that the simulation could run at only 30 frames per second with 250 active boids, as shown below.

![FPS](Images/FPSBefore.png)



The plan is to support over 1000 active boids while maintaining a frame rate above 60 FPS. To achieve this, the next iteration of the project will focus on optimizing the boid movement algorithm. The main optimization will be offloading the movement calculations to the GPU using a compute shader. By executing these calculations in parallel outside the main rendering pipeline, the overall performance of the simulation should improve overall performance significantly. 


Another improvement to implement in the next iteration is object avoidance to the boids movement algorithm. Currently, the boids teleport to the opposite side of the bounding box if they leave the bounds, and  boids will ignore obstacles in the scene, moving straight through them. To check for obstacles, a raycast will be fired from the boids current position, forward, and if it hits an obstacle it will then attempt to move around it. The method for moving around the obstacle provides a significant challenge however, because if the boids simply moved in the opposite direction of the obstacle, it would not look natural at all. Therefore, using a method explained by Sebastian Lague, raycasts will be fired in an arc in front of the boid using the golden ratio to spread out the raycasts uniformly, then each raycast that does not hit an obstacle will be checked to see how far it is from the direction the boid is currently heading, and the closest one will be used as the new direction for the boid to head in. This will allow the boid to smoothly avoid obstacles by going around them rather than turning around and going in the opposite direction. 
