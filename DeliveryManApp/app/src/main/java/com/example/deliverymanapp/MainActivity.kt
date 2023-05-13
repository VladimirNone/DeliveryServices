package com.example.deliverymanapp

import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.os.Bundle
import android.view.View
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.Volley
import com.example.deliverymanapp.dto.JwtTokenInfoDTO
import com.google.gson.Gson
import org.json.JSONException
import org.json.JSONObject
import java.net.*

class MainActivity : AppCompatActivity() {
    private var mRequestQueue: RequestQueue? = null
    private var sharedPreferences :SharedPreferences? = null
    private var cookieManager: CookieManager? = null
    private var gson: Gson = Gson()
    private val nameUserId:String = "X-UserId"
    private val nameRefreshToken:String = "X-Refresh-Token"
    private var url:String = ""
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        cookieManager = CookieManager()

        CookieHandler.setDefault(cookieManager)

        url = resources.getString(R.string.url)

        mRequestQueue = Volley.newRequestQueue(this)
        sharedPreferences = getSharedPreferences("my_preference", Context.MODE_PRIVATE)

        fetchRefreshToken()

    }

    private fun saveCookieDataToSharedPreferences(responseData : JSONObject?){

        var userId = responseData?.getString("Set-Cookie")
        if (userId != null) {
            if(userId.contains("X-UserId"))
            {
                val start = userId.indexOf('=')
                val end = userId.indexOf(';')
                userId = userId.substring(start+1,end).trim('\"')
            }
        }

        var refreshToken = responseData?.getString("Set-Cookie1")
        if (refreshToken != null) {
            if(refreshToken.contains("X-Refresh-Token"))
            {
                val start = refreshToken.indexOf('=')
                val end = refreshToken.indexOf(';')
                refreshToken = refreshToken.substring(start+1,end)
            }
        }

        val editor = sharedPreferences?.edit()
        editor?.putString(nameUserId, userId)
        editor?.putString(nameRefreshToken, refreshToken)
        editor?.apply()
    }

    private fun fetchRefreshToken() {
        val userId = sharedPreferences?.getString(nameUserId, "")
        val refreshToken = sharedPreferences?.getString(nameRefreshToken, "")
        if(userId == null || refreshToken == null || userId == "" || refreshToken == ""){
            return
        }

        val userIdCookie:HttpCookie = HttpCookie(nameUserId, userId)
        userIdCookie.version = 0
        val refreshTokenCookie:HttpCookie = HttpCookie(nameRefreshToken, refreshToken)
        refreshTokenCookie.version = 0

        cookieManager?.cookieStore?.add(URI.create(url), userIdCookie)
        cookieManager?.cookieStore?.add(URI.create(url), refreshTokenCookie)

        val req = MetaRequest(
            Request.Method.POST, "${url}/auth/refreshAccessToken", null,
            { response : JSONObject? ->
                try {
                    val body = response?.getString("body")

                    val jwtToken = gson.fromJson(body, JwtTokenInfoDTO::class.java)

                    val editor = sharedPreferences?.edit()
                    editor?.putString("jwtToken", jwtToken.jwtToken)
                    editor?.apply()


                    runOnUiThread {
                        val listIntent = Intent(this@MainActivity, OrderListActivity::class.java)
                        startActivity(listIntent)
                    }
                    Toast.makeText(this@MainActivity, "Jwt токен был обновлен", Toast.LENGTH_LONG).show()
                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            {
                    error -> run {
                Toast.makeText(this@MainActivity, "Bad request", Toast.LENGTH_SHORT).show()
            }
            })

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }

    private fun fetchLogin(login:String, password: String) {

        cookieManager?.cookieStore?.removeAll()
        val jsonBody = JSONObject("{\"login\":\"${login}\", \"password\":\"${password}\"}")

        // Pass second argument as "null" for GET requests
        val req = MetaRequest(
            Request.Method.POST, "${url}/auth/login", jsonBody,
            { response : JSONObject? ->
                try {
                    saveCookieDataToSharedPreferences(response)
                    fetchRefreshToken()
                    Toast.makeText(this@MainActivity, "Авторизация завершена", Toast.LENGTH_LONG).show()
                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            {
                error -> run {
                    Toast.makeText(this@MainActivity, "Bad request", Toast.LENGTH_SHORT).show()
                }
        })

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }

    fun loginClick(view: View) {
        val loginValue : String = findViewById<EditText>(R.id.LoginEditText).text.toString()
        val passwordValue : String = findViewById<EditText>(R.id.PasswordEditText).text.toString()
        fetchLogin(loginValue, passwordValue)
    }
}