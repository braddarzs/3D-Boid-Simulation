# 3D Boids Simulation

A real-time 3D Boids Simulation built in Unity using custom-made assets and alogithms.

[Download on itch.io](https://braddarzs.itch.io/boids-simulation)· [View the scientific poster](docs/BoidsPoster.pdf) · [Explore the development branches](#development-branches)

![Underwater boids simulation](docs/images/Boids!.png) ![Underwater boids simulation2](docs/images/Boids.png)

## Overview

Each fish follows the classic boid rules of **separation, alignment and cohesion**, while also reacting to predators, food and environmental obstacles. Neighbour calculations are offloaded to an HLSL compute shader.

The result is a responsive simulation of **1,000+ boids at over 100 FPS**—approximately **20× more boid updates per second** than the original CPU implementation.

## Features

- GPU-driven boids algorithm using Unity `ComputeBuffer`s and HLSL
- Golden-angle ray sampling for 3D obstacle avoidance
- Predator–prey and food-seeking behaviours
- Interactive camera, fish, shark, food and magnet controls

**Tech:** Unity · C# · HLSL

## Development branches

| Branch | Focus |
| --- | --- |
| [`Worksheet-2`](https://github.com/braddarzs/3D-Boid-Simulation/tree/Worksheet-2) | CPU prototype, research and profiling |
| [`Worksheet-3`](https://github.com/braddarzs/3D-Boid-Simulation/tree/Worksheet-3) | Compute-shader optimisation and obstacle avoidance |
| [`Worksheet-4`](https://github.com/braddarzs/3D-Boid-Simulation/tree/Worksheet-4) | Underwater environment and player interaction |

## Controls

| Input | Action |
| --- | --- |
| `WASD` | Move the camera or magnet |
| `Shift` / `Ctrl` | Move up / down |
| `Tab` | Toggle camera lock |
| Left click | Spawn food |
| `F` / `S` | Spawn fish / sharks |

![Underwater boids simulation](docs/images/boidsFirst.png)
