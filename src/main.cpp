#include "Quadtree.h"
#include <iostream>
#include <glm/glm.hpp>
#include <glm/gtc/random.hpp>
#include <cmath>

#include "glad/glad.h"
#include <GLFW/glfw3.h>
#include "Shader.h"
#include "SpaceTime.h"
#include <string>
#include <sstream>

#include <learnopengl/filesystem.h>

#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>

#define PI 3.14

// settings
const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 800;
GLFWwindow* window;


std::vector<glm::vec2> points;
Quadtree* quadtree;
QuadtreeDatapoint* quadtreePoints;
Shader ourShader;

bool active= false;
int space_state = GLFW_RELEASE;

int D = 3;

float bhx = 0, bhy = 0, bhz = 0;
float rate;
float rs = 0.1;
float playerDistance = -4;

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow *window);

unsigned int texture0, texture1;

int createWindow() {
	 // glfw: initialize and configure
    // ------------------------------
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    // glfw window creation
    // --------------------
    glfwWindowHint(GLFW_SAMPLES, 4);
    window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "LearnOpenGL", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    // glfwSetInputMode(window, GLFW_STICKY_KEYS, 1);

    // glad: load all OpenGL function pointers
    // ---------------------------------------
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
}

void loadShader() {
    if (D == 2) {
        ourShader = Shader("vert2.vs", "frag2.fs"); 
    }
    if (D == 3) {
        ourShader = Shader("vert3.vs", "frag3.fs"); 
    }
}

void loadTexture(const std::string& file, unsigned int& texture) {
    // load and create a texture 
    // -------------------------
    
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture); // all upcoming GL_TEXTURE_2D operations now have effect on this texture object
    // set the texture wrapping parameters
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);   // set texture wrapping to GL_REPEAT (default wrapping method)
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    // set texture filtering parameters
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    // load image, create texture and generate mipmaps
    int width, height, nrChannels;
    // The FileSystem::getPath(...) is part of the GitHub repository so we can find files on any IDE/platform; replace it with your own image path.
    unsigned char *data = stbi_load(FileSystem::getPath(file).c_str(), &width, &height, &nrChannels, 0);
    if (data)
    {
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
        glGenerateMipmap(GL_TEXTURE_2D);
    }
    else
    {
        std::cout << "Failed to load texture" << std::endl;
    }
    stbi_image_free(data);   

}

int main2()
{
	createWindow();
 	loadShader(); 

 	quadtree->setup_leafs();
 	quadtree->setup();

    int nbFrames;
    float lastTime, currentTime = 0;
    std::ostringstream display;


    loadTexture("assets/galaxy1.png", texture0);
    loadTexture("assets/galaxy2.png", texture1);

    ourShader.use();
    ourShader.setInt("texGalaxy1", 0);
    ourShader.setInt("texGalaxy2", 1);

    while (!glfwWindowShouldClose(window))
    {
        processInput(window);

        currentTime  = glfwGetTime();
        nbFrames++;
        if ( currentTime - lastTime >= 1.0 ){ // If last prinf() was more than 1 sec ago
            // printf and reset timer
            display.str("");
            display.clear();
            display  << nbFrames;
            glfwSetWindowTitle(window,display.str().c_str());
            nbFrames = 0;
            lastTime += 1.0;
        }

        // if (rs < 0.01) 
        // {
        //     active = false;
        // }
        // else {
        //     active = true;
        // }

        glClearColor(0.2f, 0.2f, 0.2f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);

        glActiveTexture(GL_TEXTURE0);
        glBindTexture(GL_TEXTURE_2D, texture0);
        glActiveTexture(GL_TEXTURE1);
        glBindTexture(GL_TEXTURE_2D, texture1);

        // glBindTexture(GL_TEXTURE_2D, texture);
        ourShader.use();

        float timeValue = glfwGetTime();
        float redValue = sin(timeValue+2) / 2.0f + 0.5f;
        float greenValue = sin(timeValue+4) / 2.0f + 0.5f;
        float blueValue = sin(timeValue) / 2.0f + 0.5f;
        int vertexColorLocation = glGetUniformLocation(ourShader.ID, "ourColor");
        glUniform3f(vertexColorLocation, redValue, greenValue, blueValue);

        int timeLocation = glGetUniformLocation(ourShader.ID, "time");
        glUniform1f(timeLocation,timeValue);

        int radiusLocation = glGetUniformLocation(ourShader.ID, "rs");
        glUniform1f(radiusLocation,rs);

        int activeLocation = glGetUniformLocation(ourShader.ID,"bh_active");
        glUniform1i(activeLocation,active);

        int bhLocation = glGetUniformLocation(ourShader.ID, "bh");
        glUniform3f(bhLocation, bhx, bhy, bhz);

        int playerLocation = glGetUniformLocation(ourShader.ID, "player");
        glUniform3f(playerLocation, 0, 0, playerDistance);


		quadtree->render_leafs();

        glfwSwapBuffers(window);
        glfwPollEvents();
    }

    glfwTerminate();
    return 0;
}

// process all input: query GLFW whether relevant keys are pressed/released this frame and react accordingly
// ---------------------------------------------------------------------------------------------------------
void processInput(GLFWwindow *window)
{
    if (glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_PRESS ) {
        rate = 0.005;
    }
    if (glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_RELEASE ) {
        rate = 0.001;
    }
    if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS ) {
        bhx -= rate;
    }
    if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS ) {
        bhx += rate;
    }
    if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS ) {
        bhy += rate;
    }
    if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS ) {
        bhy -= rate;
    }
    if (glfwGetKey(window, GLFW_KEY_HOME) == GLFW_PRESS ) {
        playerDistance += 10*rate;
    }
    if (glfwGetKey(window, GLFW_KEY_END) == GLFW_PRESS ) {
        playerDistance -= 10*rate;
    }
    if (glfwGetKey(window, GLFW_KEY_PAGE_UP) == GLFW_PRESS ) {
        rs += rate;
    }
    if (glfwGetKey(window, GLFW_KEY_PAGE_DOWN) == GLFW_PRESS ) {
        rs -= rate;
        rs = std::max(rs, 0.05f);
    }
    if (glfwGetKey(window, GLFW_KEY_R) == GLFW_PRESS) {
        loadShader();
    }
    if (glfwGetKey(window, GLFW_KEY_2) == GLFW_PRESS) {
        D = 2;
        loadShader();
    }
    if (glfwGetKey(window, GLFW_KEY_3) == GLFW_PRESS) {
        D = 3;
        bhx = 0, bhy = 0;
        loadShader();
    }
    if (space_state == GLFW_RELEASE) {
        if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS){
            glfwSetWindowShouldClose(window, true);
        }
        if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS) {
            active = !active;
        }
        space_state = GLFW_PRESS;
    }
    else {
        if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_RELEASE &&
            glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_RELEASE &&
            glfwGetKey(window, GLFW_KEY_R) == GLFW_RELEASE &&
            glfwGetKey(window, GLFW_KEY_2) == GLFW_RELEASE &&
            glfwGetKey(window, GLFW_KEY_3) == GLFW_RELEASE) {
            space_state = GLFW_RELEASE;
        }
    }
}

// glfw: whenever the window size changed (by OS or user resize) this callback function executes
// ---------------------------------------------------------------------------------------------
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    glViewport(0, 0, width, height);
}


int main() {

	srand(time(0));
	quadtree = new Quadtree(glm::vec2(0,0), glm::vec2(1,1));

	// const int nPoints = 1 * 20 * 1;
	// for(int i=0; i<nPoints; ++i) {
	// 	points.push_back(glm::vec2(glm::linearRand(-1.0f, 1.0f),glm::linearRand(-1.0f, 1.0f)));
	// }


    const int nPoints = 1 * 5;
    const int nRays = 10;
    const float a = 0.1f, b = 2.0f;
    for(int i=0; i<nPoints; ++i) {
        for(int j = 0; j < nRays; ++j) {
            float r = glm::linearRand(a, b);
            r = a*std::exp(r*std::log(b/a));
            float t = PI*glm::linearRand(-1.0f, 1.0f);
            points.push_back(glm::vec2(r*std::cos(t), r*std::sin(t)));
            // points.push_back(glm::vec2(glm::linearRand(-1.0f, 1.0f),glm::linearRand(-1.0f, 1.0f)));
        }
    }


	quadtreePoints = new QuadtreeDatapoint[nPoints*nRays];
	for(int i=0; i<nPoints*nRays; ++i) {
		quadtreePoints[i].setPosition(points[i]);
        QuadtreeDatapoint* inserted = & (quadtreePoints[i]);
		quadtree->insert( inserted );
	}


	// printf("Inserted points to quadtree\n"); fflush(stdout);
	// printf("Created %ld points\n", points.size()); fflush(stdout);

	main2();
}