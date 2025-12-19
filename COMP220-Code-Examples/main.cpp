#include <iostream>
#include <SDL.h>
#include <GL\glew.h> 
#include <SDL_opengl.h> 
#include <SDL_image.h>
#include "Shader.h" 

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#include <assimp/Importer.hpp>
#include <assimp/scene.h>
#include <assimp/postprocess.h>

#include <fstream>
#include <sstream>
#include <vector>
#include <cstdlib>
#include <ctime>

std::vector<glm::vec3> objectPositions;
float spawnTimer = 0.0f;
float spawnDelay = 1.0f;
float deltaTime = 0.016f;

struct Vertex {
	float x, y, z, u, v;
};

bool LoadModel(const char* filePath, std::vector<Vertex>& vertices, std::vector<unsigned>& indices, std::string& texturePath)
{
	Assimp::Importer importer;
	const aiScene* scene = importer.ReadFile(filePath, aiProcess_Triangulate | aiProcess_FlipUVs | aiProcess_GenSmoothNormals |
		aiProcess_GenUVCoords | aiProcess_CalcTangentSpace | aiProcess_FixInfacingNormals); //fixing up the model

//does our scene exist?
	if (!scene || scene->mFlags & AI_SCENE_FLAGS_INCOMPLETE || !scene->mRootNode || !scene->HasMeshes()) {
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "Model import failed", importer.GetErrorString(), NULL);
		return false;
	}

	//assuming just one texture
	bool hasTexture = false;
	aiString texPath("");
	if (scene->HasMaterials()) {
		aiMaterial* material = scene->mMaterials[0];
		hasTexture = material && material->GetTexture(aiTextureType_DIFFUSE, 0, &texPath) >= 0;
	}
	texturePath = texPath.C_Str(); //converts to const char
	//assuming one mesh
	aiMesh* mesh = scene->mMeshes[0];
	if (!mesh) return false; //TODO: make a sensible error message 

	vertices.clear();
	indices.clear();

	vertices.resize(mesh->mNumVertices);
	aiVector3D* texCoords = hasTexture ? mesh->mTextureCoords[0] : nullptr;
	for (unsigned i = 0; i < mesh->mNumVertices; i++)
	{
		vertices[i].x = mesh->mVertices[i].x;
		vertices[i].y = mesh->mVertices[i].y;
		vertices[i].z = mesh->mVertices[i].z;

		if (texCoords) {
			vertices[i].u = texCoords[i].x;
			vertices[i].v = texCoords[i].y;
		}
	}

	for (unsigned i = 0; i < mesh->mNumFaces; i++)
	{
		aiFace& face = mesh->mFaces[i];
		for (unsigned j = 0; j < face.mNumIndices; j++)
		{
			indices.push_back(face.mIndices[j]);
		}
	}

	return !(vertices.empty() || indices.empty());

}

bool TrySpawnObject(
	glm::vec3& outPosition,
	float& spawnTimer,
	float spawnDelay,
	float deltaTime
)
{
	spawnTimer += deltaTime;

	if (spawnTimer >= spawnDelay)
	{
		spawnTimer = 0.0f;

		outPosition = glm::vec3(
			(rand() % 10) * 3 + 20,
			(rand() % 10) * 3 + 20,
			(rand() % 10) * 3 + 20
		);

		return true;
	}
	return false;
}

int main(int argc, char** argsv)
{
	//Initialises the SDL Library, passing in SDL_INIT_VIDEO to only initialise the video subsystems
	//https://wiki.libsdl.org/SDL_Init
	if (SDL_Init(SDL_INIT_VIDEO) < 0)
	{
		//Display an error message box
		//https://wiki.libsdl.org/SDL_ShowSimpleMessageBox
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "SDL_Init failed", SDL_GetError(), NULL);
		return 1;
	}

	//Create a window, note we have to free the pointer returned using the DestroyWindow Function
	//https://wiki.libsdl.org/SDL_CreateWindow
	SDL_Window* window = SDL_CreateWindow("SDL2 Window", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 800, 640, SDL_WINDOW_OPENGL);
	//Checks to see if the window has been created, the pointer will have a value of some kind
	if (window == nullptr)
	{
		//Show error
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "SDL_CreateWindow failed", SDL_GetError(), NULL);
		//Close the SDL Library
		//https://wiki.libsdl.org/SDL_Quit
		SDL_Quit();
		return 1;
	}

	std::vector<Vertex> vertices;
	std::vector<unsigned> indices;
	std::string texturePath;

	if (!LoadModel("crate.fbx", vertices, indices, texturePath))
	{
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "ERROR LOADING MODEL", "", NULL);
		//TODO clean up here
		return 1;
	}


	SDL_Surface* image = IMG_Load(texturePath.c_str());			//IMG_Load("Crate.jpg");

	if (!image) {
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "IMG_Load failed",
			IMG_GetError(), NULL);

#pragma region I copied this from below, save by copying it
		//Destroy the window and quit SDL2, NB we should do this after all cleanup in this order!!!
		//https://wiki.libsdl.org/SDL_DestroyWindow
		SDL_DestroyWindow(window);
		//https://wiki.libsdl.org/SDL_Quit
		SDL_Quit();
#pragma endregion

		return 1;
	}

	SDL_SetRelativeMouseMode(SDL_TRUE);

	SDL_GLContext glContext = SDL_GL_CreateContext(window); 
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3); 
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 1); 

	SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE); 

	glewExperimental = GL_TRUE; 
	GLenum glewError = glewInit(); 
	if (glewError != GLEW_OK) { 
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "Unable to initialise GLEW",
			(char*)glewGetErrorString(glewError),NULL); 
	} 

	//create VAO 
	GLuint VertexArrayID; 
	glGenVertexArrays(1, &VertexArrayID); 
	glBindVertexArray(VertexArrayID); 

/*   //WE ARE NOW GETTING THIS DATA FROM A MODEL INSTEAD!
	// An array of 3 vectors which represents 3 vertices 
	static const GLfloat g_vertex_buffer_data[] = {
		//pos					//col					//texture
	   -.75f, -.75f, 0.0f,		1.0f, 0.0f, 0.0f,		0.f, 0.f,
	    .75f,  -.75f, 0.0f,		0.0f, 1.0f, 0.0f,		1.f, 0.f,
	    .75f,  .75f, 0.0f,		0.0f, 0.0f, 1.0f,		1.f, 1.f,
	   -.75f,  .75f, 0.0f,		1.0f, 1.0f, 0.0f,		0.f, 1.f
	};

	static const GLuint g_vertex_indices[] = {
		0,1,2,	// first triangle, BCD
		2,3,0	// second triangle, DAB
	};
*/ 
	// This will identify our vertex buffer
	GLuint vertexbuffer; 
	// Generate 1 buffer, put the resulting identifier in vertexbuffer
	glGenBuffers(1, &vertexbuffer); 
	// The following commands will talk about our 'vertexbuffer' buffer
	glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer); 
	// Give our vertices to OpenGL.
	glBufferData(GL_ARRAY_BUFFER, sizeof(Vertex) * vertices.size(),  
		&vertices[0], GL_STATIC_DRAW);  
	// 1st attribute buffer : vertices
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(
		0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader. 
		3,                  // size 
		GL_FLOAT,           // type 
		GL_FALSE,           // normalized? 
		sizeof(Vertex),                  // stride  
		(void*)0            // array buffer offset 
	);

/* 
	glEnableVertexAttribArray(1);
	//glBindBuffer(GL_ARRAY_BUFFER, vertexbuffer);
	glVertexAttribPointer(
		1,                  // attribute 0. No particular reason for 0, but must match the layout in the shader. 
		3,                  // size 
		GL_FLOAT,           // type 
		GL_FALSE,           // normalized? 
		8 * sizeof(GL_FLOAT),                   // stride 
		(void*)(3 * sizeof(GL_FLOAT))          // array buffer offset 
	);
*/ 
	glEnableVertexAttribArray(2);
	glVertexAttribPointer(
		2,                  // attribute 0. No particular reason for 0, but must match the layout in the shader. 
		2,                  // size 
		GL_FLOAT,           // type 
		GL_FALSE,           // normalized? 
		sizeof(Vertex),                   // stride  
		(void*)(3 * sizeof(GL_FLOAT))          // array buffer offset  
	);

	
	GLuint elementbuffer;
	glGenBuffers(1, &elementbuffer);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementbuffer);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(unsigned) * indices.size(), &indices[0], GL_STATIC_DRAW); 

	if (image) {
		// texture
		GLuint textureID;
		glGenTextures(1, &textureID);

		// "Bind" the newly created texture : all future texture functions will modify this texture
		glBindTexture(GL_TEXTURE_2D, textureID);

		// taken from: 
		int Mode = GL_RGB;

		if (image->format->BytesPerPixel == 4) {
			Mode = GL_RGBA;
		}

		glTexImage2D(GL_TEXTURE_2D, 0, Mode, image->w, image->h, 0, Mode, GL_UNSIGNED_BYTE, image->pixels);

		// Nice trilinear filtering.
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
		glGenerateMipmap(GL_TEXTURE_2D);

	}
	//shader set up 
	// Create and compile our GLSL program from the shaders
	GLuint programID = LoadShaders("vertShader.glsl", "fragShader.glsl"); 

	// Transforms
	glm::mat4 model = glm::mat4(1.0f);
	//model = glm::rotate(model, glm::radians(20.0f), glm::vec3(0.0f, 0.0f, 1.0f));
	model = glm::scale(model, glm::vec3(0.1f, 0.1f, 0.1f)); 
	//model = glm::translate(model, glm::vec3(1.0f, 1.0f, 1.0f));

	glm::mat4 mvp, view, projection;
	glm::vec3 position(0,0,5), forward(0,0,-1), left(-1,0,0), up(0,1,0), rotation(0);
	const glm::vec4 cameraFace(0, 0, -1, 0);
	const float walkSpeed = 0.5f, rotSpeed = 0.1f;

	float yaw = -90.0f;
	float pitch = 0.0f;
	const float mouseSensitivity = 0.1f;

	unsigned int transformLoc = glGetUniformLocation(programID, "transform");

	glEnable(GL_BLEND); 
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA); 
	glEnable(GL_DEPTH_TEST); 

	//Event loop, we will loop until running is set to false, usually if escape has been pressed or window is closed
	bool running = true;
	//SDL Event structure, this will be checked in the while loop
	SDL_Event ev;
	while (running)
	{
		//Poll for the events which have happened in this frame
		//https://wiki.libsdl.org/SDL_PollEvent
		while (SDL_PollEvent(&ev))
		{
			//Switch case for every message we are intereted in
			switch (ev.type)
			{
				//QUIT Message, usually called when the window has been closed
			case SDL_QUIT:
				running = false;
				break;
				//KEYDOWN Message, called when a key has been pressed down
			case SDL_MOUSEMOTION:
			{
				 //Update yaw and pitch based on mouse movement
				yaw += ev.motion.xrel * mouseSensitivity;
				pitch -= ev.motion.yrel * mouseSensitivity;

				//Clamp pitch to prevent flipping
				if (pitch > 89.0f) pitch = 89.0f;
				if (pitch < -89.0f) pitch = -89.0f;

				//Calculate new forward vector
				glm::vec3 direction;
				direction.x = cos(glm::radians(yaw)) * cos(glm::radians(pitch));
				direction.y = sin(glm::radians(pitch));
				direction.z = sin(glm::radians(yaw)) * cos(glm::radians(pitch));
				forward = glm::normalize(direction);
				left = glm::normalize(glm::cross(forward, glm::vec3(0, -1, 0)));
				break;
			}
			case SDL_KEYDOWN:
				//Check the actual key code of the key that has been pressed
				switch (ev.key.keysym.sym)
				{
				case SDLK_ESCAPE:
					running = false;
					break;

				case SDLK_w:
					position += walkSpeed * forward;
					break;

				case SDLK_s:
					position -= walkSpeed * forward;
					break;

				case SDLK_a:
					position += walkSpeed * left;
					break;

				case SDLK_d:
					position -= walkSpeed * left;
					break;
					
				case SDLK_q:
					position += walkSpeed * up;
					break;

				case SDLK_e:
					position -= walkSpeed * up;
					break;
				}
		}
		}
		glClearColor(0.0f, 0.0f, 0.4f, 0.0f); 
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT); 

		glUseProgram(programID); 

		view = glm::lookAt(
			position,
			position+forward,
			glm::vec3(0, 1, 0)
		);


		glm::vec3 spawnPos;
		if (TrySpawnObject(spawnPos, spawnTimer, spawnDelay, deltaTime))
		{
			objectPositions.push_back(spawnPos);
		}


		projection = glm::perspective(glm::radians(45.f), 4.0f / 3.0f, 0.1f, 100.0f);

		for (const glm::vec3& pos : objectPositions)
		{
			glm::mat4 model = glm::mat4(1.0f);
			model = glm::translate(model, pos);
			model = glm::scale(model, glm::vec3(0.01f));

			glm::mat4 mvp = projection * view * model;

			//Send data to shaders
			glUniformMatrix4fv(transformLoc, 1, GL_FALSE, glm::value_ptr(mvp));
			glDrawElements(GL_TRIANGLES, indices.size(), GL_UNSIGNED_INT, (void*)0);
		}
		
		SDL_GL_SwapWindow(window);
	}

	glDisableVertexAttribArray(0); 

	glDeleteBuffers(1, &vertexbuffer);
	glDeleteVertexArrays(1, &VertexArrayID);

	SDL_FreeSurface(image);
	SDL_GL_DeleteContext(glContext);
	

	//Destroy the window and quit SDL2, NB we should do this after all cleanup in this order!!!
	//https://wiki.libsdl.org/SDL_DestroyWindow
	SDL_DestroyWindow(window);
	//https://wiki.libsdl.org/SDL_Quit
	SDL_Quit();

	return 0;


}
