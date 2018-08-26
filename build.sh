rm ./a.out
g++ src/*.cpp -std=c++11 -Iinclude -lglfw3 -lGL -lX11 -lpthread -lXrandr -lXi -ldl -lXxf86vm -lXcursor -lXinerama
