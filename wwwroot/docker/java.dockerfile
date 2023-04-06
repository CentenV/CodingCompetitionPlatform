FROM openjdk:latest

WORKDIR /EXECUSION

ARG input_file_name
ARG class_name
ENV input_file_name=$input_file_name
ENV class_name=$class_name

COPY $input_file_name /EXECUSION

RUN javac $input_file_name

CMD java $class_name