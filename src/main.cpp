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

// unsigned int texture0, texture1;
// unsigned int frameBuffer0, frameBuffer1;

int createWindow() {
	 // glfw: initialize and configure
    // ------------------------------
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    // glfw window creation
    // --------------------
    // glfwWindowHint(GLFW_SAMPLES, 4);
    window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "LearnOpenGL", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    glfwSetInputMode(window, GLFW_STICKY_KEYS, 1);

    // tell GLFW to capture our mouse
    // glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

    // glad: load all OpenGL function pointers
    // ---------------------------------------
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }

    // glEnable(GL_DEPTH_TEST);
}

void loadShader() {
    if (D == 2) {
        ourShader = Shader("vert2.vs", "frag2.fs"); 
        ourShader.setInt("texGalaxy1", 0);
        ourShader.setInt("texDisk", 1);
    }
    if (D == 3) {
        ourShader = Shader("vert3.vs", "frag3.fs"); 
        ourShader.setInt("texGalaxy1", 0);
        ourShader.setInt("texDisk", 1);
    }
}

unsigned int loadTexture(char const * path)
{
    unsigned int textureID;
    glGenTextures(1, &textureID);

    int width, height, nrComponents;
    unsigned char *data = stbi_load(path, &width, &height, &nrComponents, STBI_rgb_alpha);
    if (data)
    {
        GLenum format;
        if (nrComponents == 1)
            format = GL_RED;
        else if (nrComponents == 3)
            format = GL_RGBA;
        else if (nrComponents == 4)
            format = GL_RGBA;

        std::cout << nrComponents << std::endl;

        glBindTexture(GL_TEXTURE_2D, textureID);
        glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);
        glGenerateMipmap(GL_TEXTURE_2D);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        stbi_image_free(data);
    }
    else
    {
        std::cout << "Texture failed to load at path: " << path << std::endl;
        stbi_image_free(data);
    }

    return textureID;
}

void loadTextureMS(const std::string& file, unsigned int& texture, unsigned int& framebuffer) {
    // load and create a texture 
    // -------------------------
    
    glGenFramebuffers(1, &framebuffer);
    glBindFramebuffer(GL_FRAMEBUFFER, framebuffer);

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
        glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, 4, GL_RGB, SCR_WIDTH, SCR_HEIGHT, GL_TRUE);
        glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, 0);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D_MULTISAMPLE, texture, 0);
  
    }
    else
    {
        std::cout << "Failed to load texture" << std::endl;
    }
    stbi_image_free(data);   
}


// process all input: query GLFW whether relevant keys are pressed/released this frame and react accordingly
// ---------------------------------------------------------------------------------------------------------
void processInput(GLFWwindow *window)
{
    if (glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_PRESS ) {
        rate = 0.0005;
    }
    if (glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == GLFW_RELEASE ) {
        rate = 0.0001;
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
        rs = std::max(rs, 0.01f);
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

int start()
{
    createWindow();
    loadShader(); 

    quadtree->setup_leafs();
    quadtree->setup();

    int nbFrames;
    float lastTime, currentTime = 0;
    std::ostringstream display;


    unsigned int texture0 = loadTexture("assets/galaxy1.png");
    unsigned int texture1 = loadTexture("assets/accretion_disk.png");

    // loadTextureMS("assets/galaxy1.png", texture0, frameBuffer0);
    // loadTextureMS("assets/galaxy2.png", texture1, frameBuffer1);

    ourShader.use();
    ourShader.setInt("texGalaxy1", 0);
    ourShader.setInt("texDisk", 1);

    while (!glfwWindowShouldClose(window))
    {
        processInput(window);

        currentTime  = glfwGetTime();
        nbFrames++;
        if ( currentTime - lastTime >= 1.0 ){ 
            display.str("");
            display.clear();
            display  << nbFrames;
            glfwSetWindowTitle(window,display.str().c_str());
            nbFrames = 0;
            lastTime += 1.0;
        }

        // glBindFramebuffer(GL_FRAMEBUFFER, 0);
        glClearColor(0.2f, 0.2f, 0.2f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);

        glActiveTexture(GL_TEXTURE0);
        glBindTexture(GL_TEXTURE_2D, texture0);
        glActiveTexture(GL_TEXTURE1);
        glBindTexture(GL_TEXTURE_2D, texture1);

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

int main() {

	srand(time(0));
	quadtree = new Quadtree(glm::vec2(0,0), glm::vec2(2,2));
    points.push_back(glm::vec2(1,1));
    points.push_back(glm::vec2(-1,1));
    points.push_back(glm::vec2(-1,-1));
    points.push_back(glm::vec2(1,-1));

	quadtreePoints = new QuadtreeDatapoint[points.size()];
	for(int i=0; i< points.size(); ++i) {
		quadtreePoints[i].setPosition(points[i]);
        QuadtreeDatapoint* inserted = & (quadtreePoints[i]);
		quadtree->insert( inserted );
	}

	start();
}