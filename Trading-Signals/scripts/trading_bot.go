package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"

	_ "github.com/lib/pq"
)

const (
	dbHost     = ""
	dbUser     = ""
	dbPassword = ""
	dbName     = ""
)

type Security struct {
	BasicPrice float64 `json:"basicPrice"`
}

func main() {
	// Connect to the database
	dbHost := os.Getenv("DB_HOST")
	dbUser := os.Getenv("DB_USER")
	dbPassword := os.Getenv("DB_PASSWORD")
	dbName := os.Getenv("DB_NAME")
	db, err := sql.Open("postgres",
		fmt.Sprintf("host=%s user=%s password=%s dbname=%s sslmode=disable", dbHost, dbUser, dbPassword, dbName))
	if err != nil {
		log.Fatal("Error connecting to the database: ", err)
	}
	defer db.Close()

	// Query all symbols from the user_trading_symbols table
	rows, err := db.Query("SELECT symbol, entry_price FROM user_trading_symbols")
	if err != nil {
		log.Fatal("Error querying user_trading_symbols: ", err)
	}
	defer rows.Close()

	for rows.Next() {
		var symbol string
		var entryPrice float64
		if err := rows.Scan(&symbol, &entryPrice); err != nil {
			log.Fatal("Error scanning row: ", err)
		}

		// Fetch data from the API
		resp, err := http.Get(fmt.Sprintf("https://services.entrade.com.vn/dnse-financial-product/securities/%s", symbol))
		if err != nil {
			log.Printf("Error fetching data for symbol %s: %v", symbol, err)
			continue
		}
		defer resp.Body.Close()

		if resp.StatusCode != http.StatusOK {
			log.Printf("Error fetching data for symbol %s: Status Code %d", symbol, resp.StatusCode)
			continue
		}

		var security Security
		if err := json.NewDecoder(resp.Body).Decode(&security); err != nil {
			log.Printf("Error decoding JSON for symbol %s: %v", symbol, err)
			continue
		}

		// Compare prices and print message
		fmt.Println("Price of %s: %s", symbol, security.BasicPrice)
		if security.BasicPrice > entryPrice*1.03 || security.BasicPrice < entryPrice*0.97 {
			fmt.Println("Signal: Buy %s for now...", symbol)
		}
	}

	if err := rows.Err(); err != nil {
		log.Fatal("Error iterating over rows: ", err)
	}
}
