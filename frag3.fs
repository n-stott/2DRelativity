#version 330 core
out vec4 FragColor;
varying vec2 position;
uniform vec3 ourColor;
uniform float time;
uniform float c = 1;

uniform vec3 bh = vec3(0., 0., 0);
uniform vec3 player = vec3(0., 0., -4.);
uniform float rs = 0.01;

uniform bool bh_active = false;


//dir is not dir, should be a vec4 with r and dir
vec4 colorAt(vec4 rdir, float t) {
	float d;
	float r = rdir.x;
	vec3 dir = rdir.yzw;
	d =  time - t;
	// d = atan(dir.y, dir.x);
	if (r <= rs && bh_active) {
		float f = 0; //1 - length(pos - bh) / rs;
		f = sqrt(f);
		// f = 0;
		return vec4(f,f,f,1);
	}
	else {
		d = 3*(dir.x) + 1*time;
		// vec3 n = 0.5*normalize(dir)+0.5;
		// return vec4(n.x, n.y, n.z, 1.);
		// d = atan((player-dir).y, (player-dir).x);
		return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
		// return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
		// return vec4(0.5*pos.x + 0.5, 0.5*pos.y + 0.5, 1.0f, 1.0f);
		// return vec4(0.5*(1+sin(d)), 0.5*(1-sin(d)), 0., 1.);
	}
}

vec3 rotate(vec3 x, vec3 u, float t) {
	mat3 W;
	W[0] = vec3(0, -u.z, u.y);
	W[1] = vec3(u.z, 0, -u.x);
	W[2] = vec3(-u.y, u.x, 0);
	mat3 R = mat3(1.0) + (sin(t) * W) + (2*sin(t/2)*sin(t/2) * W  * W);
	return R*x;
}

float w(float r) {
	return 1-rs/r;
}

float wp(float r) {
	return rs/(r*r);
}

float f(float r, float J) {
	// return ( w(r)*wp(r)*( 1 - J*J*w(r)/(r*r)) + 0.5*J*J*w(r)*w(r)* (2*w(r)/(r*r*r) - wp(r)/(r*r) )  );
	float f = w(r)*wp(r) * ( 1 - 3/2*w(r)*J*J/(r*r) ) + w(r)*w(r)*w(r)*J*J/(r*r*r);
	return f; // (rs > 0.06 ? (rs < 0.12 ? (rs-0.06)/0.06*f : f) : 0) ;
}

float df(float r, float J) {
	float h = 0.01;
	return (f(r+h,J) - f(r,J))/h;
}

float df2(float r, float rp, float J) {
	return 2*sqrt( rp*rp + f(r,J)*f(r,J)*df(r,J)*df(r,J) );
}

vec2 illusion(vec3 pos, float t) {
	float h = 0., ht = 0.;
	vec2 rv = (player-bh).xz;
	float r = length(rv);

	float eps = (0.5+rs)/2;

	vec2 ur = normalize(rv);
	vec2 ut = vec2(-ur.y, ur.x);

	vec3 v3 = normalize(pos);
	vec2 v = vec2(length(v3.xy), v3.z);
	float theta = -3.14/2; //atan(v.y, v.x);

	float rp = dot(v,ur);
	float thetap = dot(v,ut)/r;
	float J = r*r/w(r)*thetap;

	if (bh_active) {
		while (ht < t && r < 25) {
			// float delta = df2(r,rp,J);
			// h = min( 1 , max(  t/50. , 2*sqrt(eps/delta) ) );

			if (r < rs) 
				return vec2(r, theta);
				// return vec2(r*cos(theta), r*sin(theta))+bh;
			h = min( t - ht , eps*w(r+0.0001)); //
			rp += h*f(r,J);
			r += h*rp;
			theta += h*J*w(r)/(r*r);
			ht += h;
		}
		return vec2(r, theta);
	}
	else {
		return vec2(r, atan(v.y, v.x));
	}

}
void main()
{
	// float retard = length(position-player.xy);
	vec3 ray = vec3(position.x, position.y, 1);
	float phi = atan(-ray.y, ray.x);
	vec4 rdir;

	// if (bh_active) {
		vec2 ill = illusion(ray,30);
		rdir.x = length(ill.x);
		rdir.yzw = rotate(vec3(cos(ill.y), 0, sin(ill.y)), vec3(0,0,1), phi);
		FragColor = vec4(colorAt(rdir,0));
	// }
	// }
	// else {
	// 	rdir.x = 0;
	// 	rdir.yzw = -normalize(ray);
	// 	FragColor = vec4(colorAt(rdir,0));
	// }
}