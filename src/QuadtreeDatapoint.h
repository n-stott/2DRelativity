#ifndef QuadtreeDatapoint_H
#define QuadtreeDatapoint_H

#include <glm/vec2.hpp>
#include "Glm_tensor2.h"

#include "glad/glad.h"
#include <GLFW/glfw3.h>
#include "Shader.h"
#include <iostream>
#include "Rendarable.h"

class QuadtreeDatapoint : public Rendarable {
	glm::vec2 position; 
	glm::mat2 metric;
	glm::tensor2 christoffel;

public:
	QuadtreeDatapoint() { }
	QuadtreeDatapoint(const glm::vec2 position) : position(position) {
	}

	inline const glm::vec2 getPosition() const { return position; }
	inline void setPosition(const glm::vec2 p) { position = p; }

	inline const glm::mat2 getMetric() const { return metric; }
	inline void setMetric(const glm::mat2 p) { metric = p; }

	inline const glm::tensor2 getChristoffel() const { return christoffel; }
	inline void setChristoffel(const glm::tensor2 p) { christoffel = p; }

	void setup();
	void render();

};



#endif