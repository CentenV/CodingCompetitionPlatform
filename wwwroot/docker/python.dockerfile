FROM python:latest

WORKDIR /EXECUSION

ARG input_file_name
ENV input_file_name=$input_file_name

COPY $input_file_name /EXECUSION

CMD python3 $input_file_name