package com.example.deliverymanapp

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.view.View
import android.widget.ArrayAdapter
import android.widget.AutoCompleteTextView
import android.widget.Toast
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.example.deliverymanapp.dto.OrderStateItemDTO
import com.google.gson.Gson
import org.json.JSONException
import org.json.JSONObject
import java.net.HttpCookie
import java.net.URI

class OrderListActivity : AppCompatActivity() {
    private var mRequestQueue: RequestQueue? = null
    private var states = arrayOf<OrderStateItemDTO>()
    private val gson: Gson = Gson()
    private var url:String = ""

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_order_list)

        mRequestQueue = Volley.newRequestQueue(this)
        url = resources.getString(R.string.url)

        fetchListOfStates()
    }

    private fun fetchListOfStates() {

        val req = StringRequest(
            Request.Method.GET, "${url}/main/getOrderStates",
            { response ->
                try {
                    states = gson.fromJson(response, Array<OrderStateItemDTO>::class.java)
                    // create an array adapter and pass the required parameter
                    // in our case pass the context, drop down layout , and array.
                    val arrayAdapter = ArrayAdapter(this, R.layout.dropdown_item, states.map { it.nameOfState })
                    val headerView = findViewById<View>(R.id.header)
                    // get reference to the autocomplete text view
                    val autocompleteTV = headerView.findViewById<AutoCompleteTextView>(R.id.autoCompleteTextView)
                    // set adapter to the autocomplete tv to the arrayAdapter
                    autocompleteTV.setAdapter(arrayAdapter)

                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            { error ->
                run {
                    Toast.makeText(this@OrderListActivity, "Bad request", Toast.LENGTH_SHORT).show()
                }
            })

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }
}