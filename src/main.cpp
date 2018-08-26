#include "Quadtree.h"
#include <iostream>
#include <glm/glm.hpp>
#include <glm/gtc/random.hpp>
#include <cmath>

#include "glad/glad.h"
#include <GLFW/glfw3.h>
#include "Shader.h"
#include "SpaceTime.h"

#define PI 3.14

// settings
const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 600;
GLFWwindow* window;


std::vector<glm::vec2> points;
Quadtree* quadtree;
QuadtreeDatapoint* quadtreePoints;

bool active= false;
int space_state = GLFW_RELEASE;

float bhx = 0.3, bhy = 0;
float rate;

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow *window);


int createWindow() {
	 // glfw: initialize and configure
    // ------------------------------
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    // glfw window creation
    // --------------------
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

    // glad: load all OpenGL function pointers
    // ---------------------------------------
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
}

int main2()
{
	createWindow();
 	Shader ourShader("vert.vs", "frag.fs"); 

 	quadtree->setup_leafs();
 	quadtree->setup();

    while (!glfwWindowShouldClose(window))
    {
        processInput(window);

        glClearColor(0.2f, 0.2f, 0.2f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);

        ourShader.use();

        float timeValue = glfwGetTime();
        float redValue = sin(timeValue+2) / 2.0f + 0.5f;
        float greenValue = sin(timeValue+4) / 2.0f + 0.5f;
        float blueValue = sin(timeValue) / 2.0f + 0.5f;
        int vertexColorLocation = glGetUniformLocation(ourShader.ID, "ourColor");
        glUniform3f(vertexColorLocation, redValue, greenValue, blueValue);

        int timeLocation = glGetUniformLocation(ourShader.ID, "time");
        glUniform1f(timeLocation,timeValue);

        int activeLocation = glGetUniformLocation(ourShader.ID,"bh_active");
        glUniform1i(activeLocation,active);

        int bhLocation = glGetUniformLocation(ourShader.ID, "bh");
        glUniform2f(bhLocation, bhx, bhy);

        // std::cout << greenValue << std::endl;

		quadtree->render_leafs();
		// quadtree->render();

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
    if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS ) {
        bhy += rate;
    }
    if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS ) {
        bhy -= rate;
    }
    if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS ) {
        bhx -= rate;
    }
    if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS ) {
        bhx += rate;
    }
    if (space_state == GLFW_RELEASE) {
        if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS){
            glfwSetWindowShouldClose(window, true);
            space_state = GLFW_PRESS;
        }
        if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS) {
            active = !active;
            space_state = GLFW_PRESS;
            // std::cout << active << std::endl;
        }
    }
    else {
        if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_RELEASE && glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_RELEASE) {
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


	printf("Inserted points to quadtree\n"); fflush(stdout);
	printf("Created %ld points\n", points.size()); fflush(stdout);

	main2();
}