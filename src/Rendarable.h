#ifndef RENDARABLE_H
#define RENDARABLE_H

class Rendarable {
    public :

	    unsigned int VAO, VBO;
	    virtual void setup() = 0;
	    virtual void render() = 0;
};

#endif