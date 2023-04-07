FROM node:latest

WORKDIR /EXECUSION

ARG input_file_name
ENV input_file_name=$input_file_name

COPY $input_file_name /EXECUSION

CMD node $input_file_name