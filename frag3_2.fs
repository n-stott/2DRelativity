#version 330 core
out vec4 FragColor;
in vec2 position;
uniform vec3 ourColor;
uniform float time;
uniform float c = 1;

uniform vec3 blackHole = vec3(0,0,0);
uniform float rs = 0.1;

uniform vec3 player = vec3(0., 0., -4.);
uniform vec3 front; // = vec3(0., 0., 0);
uniform vec3 up;
uniform vec3 right;


uniform int N  = 20; // number of integration steps

uniform sampler2D texGalaxy1;
uniform sampler2D texDisk;

uniform bool optimizeFarRays = true;

const float PI = 3.14159265359;
const float TWOPI = 6.28318530718;


vec3 rotate(vec3 x, vec3 u, float t) {
	mat3 W;
	W[0] = vec3(0, -u.z, u.y);
	W[1] = vec3(u.z, 0, -u.x);
	W[2] = vec3(-u.y, u.x, 0);
	float st2 = sin(t/2);
	mat3 R = (sin(t) * W) + (2*st2*st2 * W  * W);
	return x + R*x;
}

//impact parameter - rayDir is normalized
float getB(vec3 rayPos, vec3 rayDir) {
	vec3 rv = blackHole - rayPos;
	return length( rv - dot(rayDir,rv)*rayDir );
}

float defl(float r, float r0) {
	return asin(r0/r) - 0.5*rs/r0*(2-sqrt(1-(r0/r)*(r0/r)) - sqrt((r-r0)/(r+r0))); 
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
	float b = getB(rayPos, rayDir)/rs;
	float alpha = root(b);
	vec4 rdir;
	if (b <= sqrt(27)/2) { 
		rdir.x = 0;
		return rdir;
	}
	float beta = length(rayPos - blackHole)/rs;
	vec2 v = vec2(length(rayDir.xy), rayDir.z);
	float theta = atan(v.y,v.x);

	if (dot(rayPos - blackHole, rayDir) < 0) {
		theta += integrate(alpha, beta, b)+integrate0(alpha, b)-PI;
	}
	else {
		theta += integrate0(beta, b);
	}
	float phi = atan(rayDir.y, rayDir.x);
	rdir.x = 2*rs;
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

vec3 pos(float r, float theta, float phi) {
	return r*rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
}

vec3 posv(vec3 rtp) {
	return pos(rtp.x, rtp.y, rtp.z);
}

void collision(inout vec3 rtp, inout int target, float r1, float r2, float theta, float dtheta) {
	vec3 p1 = pos(r1, theta, rtp.z);
	vec3 p2 = pos(r2, theta+dtheta, rtp.z);
	float z = 0.1;
	float c = cos(TWOPI*front.y);
	float s = sin(TWOPI*front.y);
	if ((c*p2.y-s*p2.z)*(c*p1.y-s*p1.z) <= 0 && r1 >= 2.26 && r1 <= 4) {
	// if ((p2.y-z*p2.z)*(p1.y-z*p1.z) <= 0 && r1 >= 2.26 && r1 <= 4) {
		rtp = p1;
		target = 1;
	}
}

float integrateCollision(float R, float al, inout vec3 rtp, inout float target) {
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



float integrateCollision2(float r0, float b, inout vec3 rtp, inout float target) {
	float borneI = 1;
	float borneS = r0;
	float h = (borneS - borneI)/N;
	float inte = 0;
	float e = 0, r1 = 0, r2 = 0, dtheta = 0;
	for(int i = N-1; i >= 0; --i) {
		r1 = borneI + h*(i+0.5);
		r2 = r1 - h;
		dtheta = h*g2(r1,e);
		collision(rtp, target, r1, r2, inte, dtheta);
		inte += dtheta;
	}
	return inte;
}



vec4 raytraceFastCollision(vec3 rayPos, vec3 rayDir, inout vec3 rtp, inout float target) {
	float b = getB(rayPos, rayDir)/rs;
	float alpha = root(b);
	vec4 rdir;
	float beta = length(rayPos - blackHole)/rs;
	vec2 v = vec2(length(rayDir.xy), rayDir.z);
	float theta = atan(v.y,v.x);
	float phi = atan(rayDir.y, rayDir.x);

	float R = 5;

	rtp.z = phi;


	if (dot(rayPos - blackHole, rayDir) < 0) {
		if (b <= sqrt(27)/2) { 
			theta += integrateCollision2(beta, b, rtp, target);
			rdir.x = 0;
		}
		else { 
			if (alpha < R) {
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
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

vec3 toGamma(vec3 color)  { return pow(color, vec3(1.0 / 2.2)); }
vec3 toLinear(vec3 color) { return pow(color, vec3(1.2));       }

vec3 panoramaColor( vec3 pos) {
  vec2 uv = vec2(
    0.5 - atan(pos.z, pos.x) / TWOPI + front.x,
    0.5 - asin(pos.y) / PI + front.y
  );
  return toLinear(texture2D(texGalaxy1, uv).rgb);
}

//we should have p.y \sim 0
vec4 diskColor(vec3 p) {
	vec2 uv = vec2( 
		atan(p.z, p.x) / TWOPI + front.x,
		(4-length(p))/(4 - 2.26)
	 );
	vec4 c;
	c.rgb = toLinear(texture2D(texDisk, uv).rgb);
	c.a = length(c.rgb);
	return c;
}

// rdir: vec4 with r and dir
vec4 colorAt(vec4 rdir, vec4 rdirCol, float t, vec3 rtp, float target) {

	vec3 pc = panoramaColor(rdir.yzw);
	vec4 dc = diskColor(rtp);
	vec4 color = vec4(1,1,1,1);
	if (target < 0) {
		if (rdirCol.x == 0) {
			color.rgb = vec3(0,0,0);
		}
		else {
			color.rgb = pc.rgb;
		}
	}
	else {
		color.rgb = dc.rgb;
		// color.rgb = mix(pc.rgb, dc.rgb, length(dc.rgb));
	}
	return color;

}

void main()
{
	vec3 rtp = vec3(0,0,0);
	float target = -1;

	vec3 ray = vec3(position.x, position.y, -player.z/2);
	ray = normalize(ray);

	vec4 rdir = raytraceFast(player, ray);
	vec4 rdirCol = raytraceFastCollision(player, ray, rtp, target);

	FragColor = vec4(colorAt(rdir, rdirCol, 0, rtp, target));

}