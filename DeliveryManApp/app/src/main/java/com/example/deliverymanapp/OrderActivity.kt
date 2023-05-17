package com.example.deliverymanapp

import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.os.Bundle
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.example.deliverymanapp.dto.OrderInfoDTO
import com.example.deliverymanapp.dto.OrderStateItemDTO
import com.example.deliverymanapp.dto.OrdersDTO
import com.google.gson.Gson
import org.json.JSONException
import org.json.JSONObject

class OrderActivity : AppCompatActivity() {
    private var orderId : String = ""
    private var mRequestQueue: RequestQueue? = null
    private var sharedPreferences : SharedPreferences? = null
    private val gson: Gson = Gson()
    private var url:String = ""
    private var order : OrderInfoDTO?  = null
    private var delManCanChangeStateOfOrder : Boolean?  = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_order)

        mRequestQueue = Volley.newRequestQueue(this)
        sharedPreferences = getSharedPreferences("my_preference", Context.MODE_PRIVATE)
        url = resources.getString(R.string.url)

        val arguments = intent.extras

        if (arguments != null) {
            orderId = arguments["orderId"] as String
            delManCanChangeStateOfOrder = arguments["canChangeState"] as Boolean

            val buttonChangeState = findViewById<Button>(R.id.moveOrderToNextStageButton)
            if(delManCanChangeStateOfOrder == false)
                buttonChangeState.visibility = View.GONE

            LoadOrderDataToList()
        }
    }

    private fun LoadOrderDataToList(){
        val req = object : StringRequest(
            Request.Method.GET, "${url}/Order/getOrder/${orderId}",
            { response ->
                try {
                    order = gson.fromJson(response, OrderInfoDTO::class.java)

                    val orderView = findViewById<View>(R.id.orderInfo)
                    val tvOrderId = orderView.findViewById<TextView>(R.id.OrderId)
                    val tvPrice = orderView.findViewById<TextView>(R.id.Price)
                    val tvWeight = orderView.findViewById<TextView>(R.id.Weight)
                    val tvAddress = orderView.findViewById<TextView>(R.id.Address)

                    tvOrderId.text = "Id: ${order?.order?.id}"
                    tvPrice.text = "Стоимость: ${order?.order?.price} Р"
                    tvWeight.text = "Вес: ${order?.order?.sumWeight}"
                    tvAddress.text = "Адрес: ${order?.order?.deliveryAddress}"

                    val dishes = order?.orderedDishes?.map{it.dishInfo.name + " " + it.count + " шт"}?.toTypedArray() as Array<String>

                    val arrayAdapter = ArrayAdapter<String>(this,android.R.layout.simple_list_item_1, dishes)
                    val listView = findViewById<ListView>(R.id.orderDishesList)

                    listView.adapter = arrayAdapter

                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            { error ->
                run {
                    Toast.makeText(this, "Bad request", Toast.LENGTH_SHORT).show()
                }
            })
        {
            override fun getHeaders(): MutableMap<String, String> {
                val headers = HashMap<String, String>()
                headers["Authorization"] = "Bearer " + sharedPreferences?.getString("jwtToken", "")
                return headers
            }
        }

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }

    fun moveOrderToNextStageClick(view: View) {
        val jsonBody = JSONObject("{\"orderId\":\"${orderId}\"}")

        val req = object : JsonObjectRequest(
            Request.Method.POST, "${url}/Order/moveToNextStage", jsonBody,
            { response ->
                try {
                    val intent = Intent(this, OrderListActivity::class.java)
                    this.startActivity(intent);

                } catch (e: JSONException) {
                    e.printStackTrace()
                }
            },
            { error ->
                run {
                    Toast.makeText(this, "Bad request", Toast.LENGTH_SHORT).show()
                }
            })
        {
            override fun getHeaders(): MutableMap<String, String> {
                val headers = HashMap<String, String>()
                headers["Authorization"] = "Bearer " + sharedPreferences?.getString("jwtToken", "")
                return headers
            }
        }

        /* Add your Requests to the RequestQueue to execute */
        mRequestQueue!!.add(req)
    }
}