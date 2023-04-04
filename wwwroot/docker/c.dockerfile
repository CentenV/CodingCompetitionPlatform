FROM gcc:latest

WORKDIR /EXECUSION

ARG input_file_name
ENV input_file_name=$input_file_name

COPY $input_file_name /EXECUSION

RUN gcc $input_file_name

CMD ./a.out