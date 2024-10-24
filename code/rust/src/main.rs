// main.rs
extern crate redis;
use redis::{Commands, FromRedisValue, ToRedisArgs};
use core::str;
use std::collections::HashMap;
use base64::decode;
use std::time::{Duration, Instant};


fn main() {
    println!("starting rust worker");

    let client = redis::Client::open("redis://quickie-redis:6379").unwrap();
    let mut con = client.get_connection().unwrap();

    loop {
        match con.blpop::<_, (String, String)>("quickie", 0.0) {
            Ok((_, doc_string)) => {
                let start = Instant::now();
                //println!("doc is '{}'", &doc_string);

                // Traite le document reçu
                // Parse the JSON string into a HashMap
                let mut obj: HashMap<String, serde_json::Value> = serde_json::from_str(&doc_string).unwrap();

                // Add the 'md5' property to the object
                let mavalue = obj.get("file").unwrap().as_str().unwrap();
                let mut d : String = String::new();
                match decode(&mavalue) {
                    Ok(decoded_bytes) => {
                        match str::from_utf8(&decoded_bytes) {
                            Ok(decoded_str) => {
                                //println!("Decoded: {}", decoded_str);
                                d = decoded_str.to_string();
                            },
                            Err(e) => println!("Error decoding UTF-8: {}", e),
                        }
                    },
                    Err(e) => println!("Error decoding base64: {}", e),
                }

                
                // Convert the modified HashMap back into a JSON string
                let new_json_str = serde_json::to_string("test").unwrap();
                //println!("{}", new_json_str);
                
                let digest = md5::compute(d.clone());
                let digest_hex: String = format!("{:x}", digest); // Convert digest to hex string
                obj.insert("md5".to_string(), d.into());
                let duration = start.elapsed();
                obj.insert("flavor".to_string(), ("rust").into());
                obj.insert("elapsed".to_string(), (duration.as_nanos() as f32 / 1e6f32).into());
                //println!("Time elapsed in expensive_function() is: {:?}", duration.as_nanos() );//.as_millis() as f32 /1000f32);

                let result: redis::RedisResult<i32> = con.lpush("quickie-out", serde_json::to_string(&obj).unwrap()); // Explicit annotation
                // match result {
                //     //Ok(_) => ,//println!("MD5 digest pushed to Redis: {}", digest_hex),
                //     _ => println!("oops"),
                //     //Err(e) => println!("Error pushing MD5 to Redis: {}", e),
                // }
            },
            Err(e) => {
                println!("Redis error: {}", e);
                //break;  // En cas d'erreur Redis, on sort de la boucle
            }
        }
    }
}