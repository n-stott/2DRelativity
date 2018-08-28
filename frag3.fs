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
  return pow(color, vec3(2.0));
}

vec3 panoramaColor( vec3 pos)
{
  vec2 uv = vec2(
    mod(0.5 - atan(pos.z, pos.x) / TWOPI + bh.x, 1),
    mod(0.5 - asin(pos.y) / PI + bh.y, 1)
  );
  if (bh_active) {
  	return toLinear(texture2D(texGalaxy1, uv).rgb);
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

//impact parameter
float b(vec3 rayPos, vec3 rayDir) {
	vec3 rv = blackHole - rayPos;
	return length( rv - dot(rayDir,rv)*rayDir);
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
	float theta = -3.14/2;

	float rp = dot(v,ur);
	float J = r/w(r)*dot(v,ut);

	float bv = b(rayPos, rayDir);

	if (bh_active) {
		if (bv > 30*rs && optimizeFarRays) {
			theta = atan(v.y, v.x) - defl(r,bv);
		}
		else {
			float h = 0., ht = 0.;
			float eps = (0.2+rs)*w(r+0.0001);
			int i = 0;
			while (ht < 40 && r < 25 && i < 200) {
				if (r < rs) return vec4(r, theta, 0, 0);

				h = min( 40 - ht , eps*w(r+0.0001));

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

	//interpolation for small black holes
	if (bh_active && rs <= 0.03) {
		float t = (max(rs,0.01) - 0.01)/0.02;
		theta = t*theta + (1-t)*atan(v.y, v.x);
	}

	//interpolation for far rays
	if (bh_active && bv > 15*rs && optimizeFarRays) {
		float t = (min(bv,30*rs) - 15*rs)/(15*rs);
		theta = (1-t)*theta + t*atan(v.y, v.x);
	}

	float phi = atan(rayDir.y, rayDir.x);
	vec4 rdir;
	rdir.x = r;
	rdir.yzw = rotate(vec3(cos(theta), 0, sin(theta)), vec3(0,0,1), phi);
	return rdir;
}

// rdir: vec4 with r and dir
vec4 colorAt(vec4 rdir, float t) {
	float d;
	float r = rdir.x;
	vec3 dir = rdir.yzw;
	d =  time - t;
	if (r <= rs && bh_active && rs >= 0.03) {
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
	vec3 ray = vec3(position.x, position.y, -player.z/2);
	ray = normalize(ray);
	vec4 rdir = raytrace(player, ray);
	FragColor = vec4(colorAt(rdir,0));
}