#ifndef SPACETIME_H
#define SPACETIME

#include <glm/glm.hpp>



class SpaceTime : public Rendarable {
public:
	glm::vec2 center;
	float rs = 1;
	float vertices[12];

	SpaceTime() : center(0,0) {
	}

	SpaceTime(float f) : center(0,0) {
		rs = f;
	}

	~SpaceTime() {
	}

	void setup() {
	    glGenVertexArrays(1, &VAO);
	    glGenBuffers(1, &VBO);
	    glBindVertexArray(VAO);
	    float vertices[] = 	{-1. , -1 , 0 ,
			-1. , +1 , 0 ,
			+1. , +1 , 0 ,
			+1. , -1 , 0
		};
	    glBindBuffer(GL_ARRAY_BUFFER, VBO);
	    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
	    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
	    glEnableVertexAttribArray(0);
	    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
	    glEnableVertexAttribArray(1);

	}

	void render() {
	    glBindVertexArray(VAO);
	    glPointSize(3);
	    glDrawArrays(GL_POINTS, 0, 1);
	}

};

#endif