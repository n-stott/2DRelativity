#include "QuadtreeDatapoint.h"

void QuadtreeDatapoint::setup() {
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    float vertices[] = { position.x, position.y,  0.0f,  1.0f, 0.0f, 0.0f};
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
    glEnableVertexAttribArray(1);

}

void QuadtreeDatapoint::render() {
    glBindVertexArray(VAO);
    glPointSize(3);
    glDrawArrays(GL_POINTS, 0, 1);
}