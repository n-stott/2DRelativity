#ifndef Quadtree_H
#define Quadtree_H

#include "QuadtreeDatapoint.h"
#include "Rendarable.h"


class Quadtree : public Rendarable {
	glm::vec2 origin;
	glm::vec2 halfDimension;
	Quadtree* children[4];


public:
	QuadtreeDatapoint* data;

	Quadtree(const glm::vec2 origin, const glm::vec2 halfDimension) 
		: origin(origin), halfDimension(halfDimension), data(NULL) {
			for(int i=0; i<4; ++i) 
				children[i] = NULL;
		}

	Quadtree() : origin(glm::vec2(0,0)), halfDimension(glm::vec2(1,1)), data(NULL) {
			for(int i=0; i<4; ++i) 
				children[i] = NULL;
		}

	Quadtree(const Quadtree& copy)
		: origin(copy.origin), halfDimension(copy.halfDimension), data(copy.data) {
			for(int i=0; i<4; ++i) 
				children[i] = copy.children[i];
		}

	~Quadtree() { for(int i=0; i<4; ++i) delete children[i]; }

	void setup();
	void render();

	void setup_leafs();
	void render_leafs();

	int getQuadrantContainingPoint(const glm::vec2 point) const {
		int quad = 0;
		if(point.x >= origin.x) quad |= 2;
		if(point.y >= origin.y) quad |= 1;
		return quad;
	}

	Quadtree closestLeaf(const glm::vec2 point) const {
		if (isLeafNode()) {
			return *this;
		}
		else {
			return (children[getQuadrantContainingPoint(point)])->closestLeaf(point);
		}
	}

	bool isLeafNode() const {
	 return children[0] == NULL; 
	}

	void insert(QuadtreeDatapoint* point) {
		if(!fits(*point)) return;
		// std::cout << fits(*point) << std::endl;
		if(isLeafNode()) {
			if(data==NULL) {
				data = point;
				return;
			} else {
				QuadtreeDatapoint *oldPoint = data;
				data = NULL;
				for(int i=0; i<4; ++i) {
					glm::vec2 newOrigin = origin;
					newOrigin.x += halfDimension.x * (i&2 ? .5f : -.5f);
					newOrigin.y += halfDimension.y * (i&1 ? .5f : -.5f);
					children[i] = new Quadtree(newOrigin, halfDimension*.5f);
				}
				children[getQuadrantContainingPoint(oldPoint->getPosition())]->insert(oldPoint);
				children[getQuadrantContainingPoint(point->getPosition())]->insert(point);
			}
		} else {
        // std::cout << "*3" <<std::endl;
			int quadrant = getQuadrantContainingPoint(point->getPosition());
			children[quadrant]->insert(point);
		}
	}

	bool fits(const QuadtreeDatapoint p ) {
		float x = p.getPosition().x, y = p.getPosition().y;
		return ( x - origin.x <= halfDimension.x ) && (y - origin.y <= halfDimension.y ) &&
			   ( x - origin.x >= -halfDimension.x ) && ( y - origin.y >= -halfDimension.y );
	}

	void getPointsInsideBox(const glm::vec2 bmin, const glm::vec2 bmax, std::vector<QuadtreeDatapoint*>& results) {
		if(isLeafNode()) {
			if(data!=NULL) {
				const glm::vec2 p = data->getPosition();
				if(p.x>bmax.x || p.y>bmax.y) return;
				if(p.x<bmin.x || p.y<bmin.y) return;
				results.push_back(data);
			}
		} else {
			for(int i=0; i<4; ++i) {
				glm::vec2 cmax = children[i]->origin + children[i]->halfDimension;
				glm::vec2 cmin = children[i]->origin - children[i]->halfDimension;
				if(cmax.x<bmin.x || cmax.y<bmin.y) continue;
				if(cmin.x>bmax.x || cmin.y>bmax.y) continue;
				children[i]->getPointsInsideBox(bmin,bmax,results);
			} 
		}
	}


};


#endif