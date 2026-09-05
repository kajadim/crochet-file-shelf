# Frontend (Angular) - image za povremeno pokretanje cele aplikacije preko Docker-a.
# Za svakodnevni razvoj i dalje se koristi `npm start` direktno - ovo je samo "brza provera".

FROM node:22-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 4200
CMD ["npm", "start", "--", "--host", "0.0.0.0"]
