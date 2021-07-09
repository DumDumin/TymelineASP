docker run -p 3306:3306 --rm --name asp -v ~/code/hobby/TymelineASP/sql:/var/lib/mysql -e MYSQL_ROOT_PASSWORD=asdf1234 -d mysql:8
