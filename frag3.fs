#version 330 core
out vec4 FragColor;
in vec2 position;
uniform vec3 ourColor;
uniform float time;
uniform float c = 1;

uniform vec3 bh = vec3(0., 0., 0);
uniform vec3 blackHole = vec3(0,0,0);
uniform vec3 player = vec3(0., 0., -4.);
uniform float rs = 0.1;

uniform int N  = 20; // number of integration steps

uniform sampler2D texGalaxy1;
uniform sampler2D texGalaxy2;

uniform bool bh_active = false;

uniform bool optimizeFarRays = true;

const float PI = 3.14159265359;
const float TWOPI = 6.28318530718;


vec3 toGamma(vec3 color) {
  return pow(color, vec3(1.0 / 2.2));
}

vec3 toLinear(vec3 color) {
  return pow(color, vec3(1.2));
}

vec3 panoramaColor( vec3 pos)
{
  vec2 uv = vec2(
    mod(0.5 - atan(pos.z, pos.x) / TWOPI + bh.x, 1),
    mod(0.5 - asin(pos.y) / PI + bh.y, 1)
  );
  vec2 uv2 = vec2(
    mod(1.2 - atan(pos.z, pos.x) / TWOPI + bh.x, 1),
    mod(1.2 - asin(pos.y) / PI + bh.y, 1)
  );
  if (bh_active) {
  	return toLinear(texture2D(texGalaxy1, uv).rgb) + toLinear(texture2D(texGalaxy1, uv2).rgb);
  }
  else {
  	return toLinear(texture2D(texGalaxy2, uv).rgb);
  }
  	
}

vec3 rotate(vec3 x, vec3 u, float t) {
	mat3 W;
	W[0] = vec3(0, -u.z, u.y);
	W[1] = vec3(u.z, 0, -u.x);
	W[2] = vec3(-u.y, u.x, 0);
	float st2 = sin(t/2);
	mat3 R = (sin(t) * W) + (2*st2*st2 * W  * W);
	return x + R*x;
}

float w(float r) {
	return 1-rs/r;
}

float wp(float r) {
	return rs/(r*r);
}

float f(float r, float J) {
	float r2 = r*r, r3 = r2*r, wr = w(r), w3 = wr*wr*wr, wpr = wp(r), J2 = J*J;
	return wr*wpr*(1 - 3/2*wr*J2/r2) + w3*J2/r3;
}

//impact parameter - rayDir is normalized
float b(vec3 rayPos, vec3 rayDir) {
	vec3 rv = blackHole - rayPos;
	return length( rv - dot(rayDir,rv)*rayDir );
}

float defl(float r, float r0) {
	return asin(r0/r) - 0.5*rs/r0*(2-sqrt(1-(r0/r)*(r0/r)) - sqrt((r-r0)/(r+r0))); 
}

// vec2 illusion(vec3 pos, float t) {
vec4 raytrace(vec3 rayPos, vec3 rayDir) {
	vec2 rv = (rayPos-blackHole).xz;
	float r = length(rv);

	vec2 ur = normalize(rv);
	vec2 ut = vec2(-ur.y, ur.x);

	vec2 v = vec2(length(rayDir.xy), rayDir.z);
	float theta = -PI/2;

	float rp = dot(v,ur);
	float J = r/w(r)*dot(v,ut);

	float bv = b(rayPos, rayDir);

	// J = bv;

	if (bh_active) {
		if (bv > 30*rs && optimizeFarRays) {
			theta = atan(v.y, v.x) - defl(r,bv);
		}
		else {
			float h = 0., ht = 0.;
			int i = 0;
			theta =  -PI/2 ; //atan(v.y, v.x);
			while (ht < 50 && i < 1000) {
				if (r < rs) return vec4(r, theta, 0, 0);
				float eps = 0.01;
				h = min( 50 - ht , eps);

				rp += h*f(r,J);
				r += h*rp;
				theta += h*J*w(r)/(r*r);
				ht += h;
				i++;
			}
		}
	}
	else {
		theta = atan(v.y, v.x);
	}

	// theta = PI + 2*(theta - PI);

	// //interpolation for small black holes
	// if (bh_active && rs <= 0.03) {
	// 	float t = (max(rs,0.01) - 0.01)/0.02;
	// 	float s = t*t*(3 - 2*t);
	// 	theta = s*theta + (1-s)*atan(v.y, v.x);
	// }

	// //interpolation for far rays
	// if (bh_active && bv > 15*rs && optimizeFarRays) {
	// 	float t = (min(bv,30*rs) - 15*rs)/(15*rs);
	// 	float s = t*t*(3 - 2*t);
	// 	theta = (1-s)*theta + s*atan(v.y, v.x);
	// }

	float phi = atan(rayDir.y, rayDir.x);
	vec4 rdir;
	rdir.x = r;
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

float eq(float r, float b) {
	float b2 = b*b;
	return b2+r*( -b2 + r*r );
}

float root(float b) {
	float x = b/sqrt(3);
	float y = b;
	for(int i = 0; i < 20; i++) {
		float z = eq(0.5*(x+y),b);
		if (z < 0) {
			x = 0.5*(x+y);
		}
		else y = 0.5*(x+y);
	}
	return y;
}

float g(float al, float eps) {
	float e2 = eps*eps, e4 = e2*e2;
	return 2/sqrt( (2*al - 3)/(al*al) + e2*(3/al-1) - e4 );
}

float integrate(float al, float be, float b) {
	float borne = sqrt(1/al - 1/be);
	float h = borne/N;
	float inte = 0;
	for(int i = 0; i < N; ++i) {
		inte += h*g(al,h*(i+0.5));
	}
	return inte;
}

float integrate0(float al, float b) {
	float borne = sqrt(1/al);
	float h = borne/N;
	float inte = 0;
	for(int i = 0; i < N; ++i) {
		inte += h*g(al,h*(i+0.5));
	}
	return inte;
}

float integrate1(float al, float R, float be, float b) {
	float borneI = sqrt(1/al-1/R);
	float borneS = sqrt(1/al-1/be);
	float h = (borneS - borneI)/N;
	float inte = 0;
	for(int i = 0; i < N; ++i) {
		inte += h*g(al,borneI + h*(i+0.5));
	}
	return inte;
}

float integrate2(float al, float R, float b) {
	float borneI = sqrt(1/al-1/R);
	float borneS = sqrt(1/al);
	float h = (borneS - borneI)/N;
	float inte = 0;
	for(int i = 0; i < N; ++i) {
		inte += h*g(al,borneI + h*(i+0.5));
	}
	return inte;
}


float g2(float r, float b) {
	float r2 = r*r;
	float b2 = b*b;
	float hi = r2*sqrt(1/b2 - (1-1/r)/r2);
	return 1/hi;
}


vec4 raytraceFast(vec3 rayPos, vec3 rayDir) {
	if (rayDir.x > 0 ) {
		// return raytrace(rayPos, rayDir);
	}
	float b = b(rayPos, rayDir)/rs;
	float alpha = root(b);
	vec4 rdir;
	if (b <= sqrt(27)/2) { 
		rdir.x = 0;
		return rdir;
	}
	float beta = length(rayPos - blackHole)/rs;
	vec2 v = vec2(length(rayDir.xy), rayDir.z);
	float theta = atan(v.y,v.x);

	if (bh_active) {
		if (dot(rayPos - blackHole, rayDir) < 0) {
			theta += integrate(alpha, beta, b)+integrate0(alpha, b)-PI;
		}
		else {
			theta += integrate0(beta, b);
		}
	}
	float phi = atan(rayDir.y, rayDir.x);
	rdir.x = 2*rs;
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

vec3 pos(float r, float theta, float phi) {
	return r*rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
}

void collision(inout vec3 rtp, inout int target, float r1, float r2, float theta, float dtheta) {
	vec3 p1 = pos(r1, theta, rtp.z);
	vec3 p2 = pos(r2, theta+dtheta, rtp.z);
	float z = 0.1;
	if ((p2.y-z*p2.z)*(p1.y-z*p1.z) <= 0 && r1 >= 4 && r1 <= 8) {
		rtp = vec3(1,1,1);
		target = 1;
	}
}

float integrateCollision(float R, float al, inout vec3 rtp, inout int target) {
	float borne = sqrt(1/al - 1/R);
	float h = borne/N;
	float inte = 0;
	float e = 0, r1 = 0, r2 = 0, dtheta = 0;
	for(int i = N-1; i >= 0; --i) {
		e = h*(i+0.5);
		r1 = al/(1-al*e*e);
		r2 = al/(1-al*(e+h)*(e+h));
		dtheta = h*g(al,e);
		collision(rtp, target, r1, r2, inte, dtheta);
		if (target >= 0) break;
		inte += dtheta;
	}
	if (target >= 0) return inte;
	for(int i = 0; i < N; ++i) {
		e = h*(i+0.5);
		r1 = al/(1-al*e*e);
		r2 = al/(1-al*(e+h)*(e+h));
		dtheta = h*g(al,e);
		collision(rtp, target, r1, r2, inte, dtheta);
		if (target >= 0) break;
		inte += dtheta;
	}
	return inte;
}



float integrateCollision2(float r0, float b, inout vec3 rtp, inout int target) {
	float borneI = rs;
	float borneS = r0;
	float h = (borneS - borneI)/N;
	float inte = 0;
	float e = 0, r1 = 0, r2 = 0, dtheta = 0;
	for(int i = N-1; i >= 0; --i) {
		r1 = rs + h*(i+0.5);
		r2 = r1 - h;
		dtheta = h*g2(r1,e);
		collision(rtp, target, r1, r2, inte, dtheta);
		inte += dtheta;
	}
	return inte;
}



vec4 raytraceFastCollision(vec3 rayPos, vec3 rayDir, inout vec3 rtp, inout int target) {
	if (rayDir.x > 0 ) {
		// return raytrace(rayPos, rayDir);
	}
	float b = b(rayPos, rayDir)/rs;
	float alpha = root(b);
	vec4 rdir;
	// if (b <= sqrt(27)/2) { 
	// 	rdir.x = 0;
	// 	return rdir;
	// }
	float beta = length(rayPos - blackHole)/rs;
	vec2 v = vec2(length(rayDir.xy), rayDir.z);
	float theta = atan(v.y,v.x);
	float phi = atan(rayDir.y, rayDir.x);

	float R = 10;

	rtp.z = phi;


	if (bh_active) {
		if (dot(rayPos - blackHole, rayDir) < 0) {
			if (b <= sqrt(27)/2) { 
				theta += integrateCollision2(beta, b, rtp, target);
				rdir.x = 0;
			}
			else { 
				if (alpha < R) {
					//incorporate phi in the collision test
					theta += integrate1(alpha, R, beta, b) + integrateCollision(R, alpha, rtp, target) + integrate2(alpha, R, b)-PI;
				}
				else {
					theta += integrate(alpha, beta, b)+integrate0(alpha, b)-PI;
				}
				rdir.x = 2*rs;
			}
		}
		else {
			theta += integrate0(beta, b);
		}
	}
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

// rdir: vec4 with r and dir
vec4 colorAt(vec4 rdir, float t) {
	float d;
	float r = rdir.x;
	vec3 dir = rdir.yzw;
	d =  time - t;
	// if (r <= rs && bh_active && rs >= 0.03) {
	if (r <= rs && bh_active) {
		float f = 0.0;
		f = sqrt(f);
		return vec4(f,f,f,1);
	}
	else {
		vec3 c = panoramaColor(rdir.yzw);
		return vec4(c.r, c.g, c.b, 1);
		d = 6*(dir.y) + 1*time;
		return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
	}
}

void main()
{
	vec3 rtp = vec3(0,0,0);
	int target = -1;

	vec3 ray = vec3(position.x, position.y, -player.z/2);
	ray = normalize(ray);
	// vec4 rdir = raytraceFast(player, ray);
	vec4 rdir = raytraceFastCollision(player, ray, rtp, target);
	// vec4 rdir = raytrace(player, ray);
	if (target < 0) {
		FragColor = vec4(colorAt(rdir,0));
	}
	else {
		FragColor = mix(vec4(colorAt(rdir,0)),vec4(rtp.x,rtp.x,rtp.x,1),1);
	}
}