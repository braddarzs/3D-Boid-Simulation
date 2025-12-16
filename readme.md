
# Worksheet 3

## Implementation


### Compute Shader

A compute shader was implemented in this iteration to process the boid movement logic on the GPU.
Each GPU thread processes a single boid, identified by its dispatch thread ID.

#### Neighbour Calculation

First, the distance between the current boid and another boid is calculated using the vector offset between their positions:
```csharp
float3 offset = currentBoid.position - other.position;
float dist = length(offset);
```
Boids that are either at zero distance or outside the view radius are ignored:
```csharp
if (dist == 0 || dist > currentBoid.viewRadius)
    continue;
```
This is to ensure that only boids within a specified proximity of the boid are used to calculate its final force.

Additionally, the angle between the boid’s forward direction and the direction toward another boid is computed using the dot product:
```csharp
float angle = degrees(acos(dot(normalize(currentBoid.direction), normalize(offset))));
```
If the angle exceeds half of the boid’s total view angle, the neighbour is ignored, as shown below. The angle is halfed due to the angle calculated being relative to the boids foward direction, meaning it needs to negative to the left of foward and positive to the right.

```csharp
if (angle > currentBoid.viewAngle * 0.5f)
    continue;
```
This is to ensure that only boids within a specified cone from the boids foward direction are used to calculate its final force. 

Boids within both the view range and angle are treated as a neighbour.

#### Forces calculation

Seperation, cohesion and alignment forces are then calculated:

- Seperation- For each neighbour, if it is within the separation range, a repulsive force is added in the opposite direction, scaled by distance, to the seperation force.
```csharp
currentBoid.seperationForce += normalize(offset) / dist;
```
- Cohesion- The position of each neighbour is added to the cohesion force:
```csharp
currentBoid.cohesionForce += other.position;
```

- Alignment- For each neighbour, the neighbour’s forward direction is added to the alignment force accumulator:
```csharp
currentBoid.alignmentForce += normalize(other.direction);
```

### Boid Manager

A BoidManager class is used to send/recieve data to/from the compute shader. It first sets up a ComputeBuffer, which is the length of the boids array that is pre-set before run-time, with the stride of a custom BoidData struct.

BoidData contains the: Position, direction, view radius, view angle, seperation range and boid type of the boid.

A boidData array is then initialized, with the size of the number of boids. Then, each frame, every element of the boidData array is set to the corresponding data for a boid and then the boidData array is dispatched to the compute shader and the results are read. The number of thread groups is calculated by the formula shown below.

```csharp
const int threadGroupSize = 1024;
int threadGroups;

threadGroups = Mathf.CeilToInt(boids.Length / (float)threadGroupSize);
```
The BoidManager then sets the boids parameters to the data read from the shader, and the function UpdateBoid() is called on the boid's Boid script, to update it.

### Boid

The Boid class is used to move the boid. Once the BoidManager calls UpdateBoid(), an intial direction of the boids foward direction is set to a vector called desiredDirection. UpdateBoid() then uses the seperation, cohesion and alignment forces recieved from the BoidManager by normalizing them, multiplying them by a scalar for customizable boid logic and then adding the result to the desiredDirection vector.
```csharp
if (desiredDirection == Vector3.zero) desiredDirection = transform.forward;

//Seperation 
desiredDirection += (separationForce.normalized) * boidSettings.separationStrength;

//Alignment
desiredDirection += (alignmentForce.normalized) * boidSettings.alignmentStrength;

//Cohesion
Vector3 averageNeighbourPos = cohesionForce / neighborCount;
desiredDirection += ((averageNeighbourPos - transform.position).normalized) * boidSettings.cohesionStrength;
```
The obstacle avoidance force is then calculated, which 

## Optimization
![unity logo](Images/PerformanceAfter.png)


After the compute shader optimization, performance increased significantly, allowing for over 1000 boids at over 100fps, as shown below. This is a 20x improvement in performance, going from 7500 boid updates per second to 150,000 boid updates per second.
![unity logo](Images/FPSAfter.png)

Additionally, the computational strain on the CPU was dramatically reduced, bringing it in line with the GPU, with both the CPU and GPU harbouring roughly a 6.5ms frame time, as shown below. The frame time achieved is much better than the goal set for this iteration, showing glowing results from using the GPU to offload computation from the CPU.
![unity logo](Images/CPUtimeAfter.png)

The total main thread usage of the BoidManager update function also dropped from 75% to 33%, as shown below. This allows for additional features to be added in the next iteration, while still maintaining good performance.
![unity logo](Images/PercentageAfter.png)

## Next Iteration

Visual improvements, user interaction, controllable magnet?
