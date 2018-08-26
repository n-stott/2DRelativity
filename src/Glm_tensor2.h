#ifndef Glm_tensor2_H
#define Glm_tensor2_H

#include <glm/glm.hpp>
#include <vector>

namespace glm {
	class tensor2 {

		std::vector<glm::mat2> t;
		
		public:
			tensor2() { t = std::vector<glm::mat2> { glm::mat2(0.,0.,0.,0.) , glm::mat2(0.,0.,0.,0.) }; }
			tensor2(std::vector<glm::mat2> tref) { t = tref; }
			tensor2(glm::mat2 a, glm::mat2 b) { t = std::vector<glm::mat2> { a , b }; }

			float operator()(bool x, bool y, bool z) { return t[x][y][z]; }

		};
}

#endif