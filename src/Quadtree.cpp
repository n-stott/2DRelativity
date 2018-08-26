#include "Quadtree.h"

void Quadtree::setup_leafs() {
    if(data == NULL) {
    }
    else {
        data->setup();
    }
    if (!isLeafNode()) {
        for(int  i = 0; i < 4; ++i) {
        children[i]->setup_leafs();
        children[i]->setup();
        }
    }
}



void Quadtree::render_leafs() {
    if(data == NULL) {
    }
    else {
        data->render();
    }
    if (!isLeafNode()) {
        for(int  i = 0; i < 4; ++i) {
        children[i]->render_leafs();
        children[i]->render();
        }
    }
}

void Quadtree::setup() {
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glm::vec2 o = origin, d = halfDimension;

    float vertices[] = { 
        o.x + d.x, o.y + d.y, 0.0f,
        o.x - d.x, o.y + d.y, 0.0f,
        o.x - d.x, o.y - d.y, 0.0f,
        o.x + d.x, o.y - d.y, 0.0f
    };
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);
    // glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
    // glEnableVertexAttribArray(1);

}

void Quadtree::render() {
    glBindVertexArray(VAO);
    glLineWidth(0.5);
    // glDrawArrays(GL_LINE_LOOP, 0, 4);
    glDrawArrays(GL_TRIANGLE_FAN, 0, 4);
}